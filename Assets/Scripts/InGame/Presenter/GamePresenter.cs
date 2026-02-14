using UnityEngine;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;

using Initial;
using InGame.Model;
using InGame.Model.Logic;
using InGame.Model.Interface;
using InGame.Model.Data;
using InGame.View;

namespace Presenter
{
    public class GamePresenter : IStartable, ITickable, IFixedTickable, IDisposable
    {
        private GameModel _gameModel;

        private PuzzleRule _puzzleRule;

        private ChainManager _chainManager;
        private TimeTsumSpawnManager _timeManager;
        private PuzzleManager _puzzleManager;
        private TsumPhysicsManager _tsumPhysicsManager;
        private SkillManager _skillManager;

        private GameUIView _gameUIView;
        private SEView _seView;
        private TsumSpawner _tsumSpawner;
        private ParticleSpawner _particleSpawner;
        private ReadyAnimationEvent _readyAnimationEvent;
        private InputEventHandler _inputEventHandler;
        private ChainLineHandler _chainLineHandler;
        private PhysicsBoundary _physicsBoundary;
        private GameoverManager _gameoverManager;

        private GameData _gameData;
        private TsumData _tsumData;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public GamePresenter(
            GameModel gameModel,
            ChainManager chainManager,
            TimeTsumSpawnManager timeManager,
            PuzzleManager puzzleManager,
            TsumPhysicsManager tsumPhysicsManager,
            SkillManager skillManager,
            PuzzleRule puzzleRule,
            GameUIView gameUIView,
            SEView seView,
            TsumSpawner tsumSpawner,
            ParticleSpawner particleSpawner,
            InputEventHandler inputEventHandler,
            ReadyAnimationEvent readyAnimationEvent,
            ChainLineHandler chainLineHandler,
            PhysicsBoundary physicsBoundary,
            GameoverManager gameoverManager,
            GameData gameData,
            TsumData tsumData
        )
        {
            _gameModel = gameModel;
            _chainManager = chainManager;
            _timeManager = timeManager;
            _puzzleManager = puzzleManager;
            _tsumPhysicsManager = tsumPhysicsManager;
            _skillManager = skillManager;
            _puzzleRule = puzzleRule;
            _gameUIView = gameUIView;
            _seView = seView;
            _tsumSpawner = tsumSpawner;
            _particleSpawner = particleSpawner;
            _inputEventHandler = inputEventHandler;
            _readyAnimationEvent = readyAnimationEvent;
            _chainLineHandler = chainLineHandler;
            _physicsBoundary = physicsBoundary;
            _gameoverManager = gameoverManager;
            _gameData = gameData;
            _tsumData = tsumData;
        }

        public void Start()
        {
            StartAsync().Forget();
        }

        private async UniTaskVoid StartAsync()
        {
            await UniTask.WhenAll(
                _tsumSpawner.Initialize(_gameUIView, _seView, _gameData.MaxTsumCount),
                _particleSpawner.Initialize(_gameData.MaxDeletedTsumEffectCount)
            );

            _gameUIView.SetParticleSpawner(_particleSpawner);
            _timeManager.Initialize();

            BindGameState();
            BindModelDataUpdate();
            BindViewUpdate();

            try
            {
                FadeMaskManager.FadeOut().Forget();
            }
            catch (Exception ex) { }

            StartGame();
        }

        public void Tick()
        {
            if (_gameModel.CurrentGameState.Value != GameModel.GameState.Playing)
            {
                return;
            }

            if (_gameModel.IsSkillActivationReady.Value)
            {
                return;
            }

            float alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            foreach (var tsum in _puzzleManager.AllTsums)
            {
                Vector2 interpolatedPos = _tsumPhysicsManager.GetInterpolatedPosition(tsum.PhysicsIndex, alpha);
                float interpolatedRotation = _tsumPhysicsManager.GetInterpolatedRotation(tsum.PhysicsIndex, alpha);
                tsum.TsumView.UpdateTransform(interpolatedPos, interpolatedRotation);
            }

            if (_puzzleManager.IsSelectionActive)
            {
                SelectTsum();
            }

            if (_timeManager.Tick(Time.deltaTime))
            {
                int randomTsumId = _puzzleRule.GetRandomTsumID(_gameData.MaxSpawnTsumLevelIndex, _tsumData);
                Vector2 spawnPos = _tsumSpawner.GetRandomSpawnPosition();
                _puzzleManager.CreateTsum(randomTsumId, spawnPos);
            }
        }

        public void FixedTick()
        {
            if (_gameModel.CurrentGameState.Value != GameModel.GameState.Playing)
            {
                return;
            }

            if (_gameModel.IsSkillActivationReady.Value)
            {
                return;
            }

            _tsumPhysicsManager.UpdateAllTsumPosition(Time.fixedDeltaTime, _physicsBoundary.LeftX, _physicsBoundary.RightX, _physicsBoundary.BottomY, _physicsBoundary.TopY);
            _tsumPhysicsManager.SetGameoverTargetByHeight(_physicsBoundary.DeadLineY);
            bool existTsumAboveDeadLine = _tsumPhysicsManager.ExistTsumAboveDeadLine(_physicsBoundary.DeadLineY);
            _gameUIView.UpdateDeadLineAlpha(_gameoverManager.GetGraceProgress(_gameData.GameOverGraceTime));
            if (_gameoverManager.IsGameover(Time.fixedDeltaTime, existTsumAboveDeadLine, _gameData.GameOverGraceTime))
            {
                _gameModel.CurrentGameState.Value = GameModel.GameState.GameOver;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void BindGameState()
        {
            _gameModel.CurrentGameState
                .DistinctUntilChanged()
                .Subscribe(async state =>
                {
                    switch (state)
                    {
                        case GameModel.GameState.Ready:
                            {
                                _gameUIView.PlayReadyAnimation();
                                break;
                            }
                        case GameModel.GameState.Playing:
                            {
                                SpawnInitialTsums();
                                break;
                            }
                        case GameModel.GameState.GameOver:
                            {
                                Debug.Log("Game Over!");
                                _seView.PlayGameOverSound();
                                await UniTask.Delay(2000);
                                _gameUIView.ShowGameOver();
                                break;
                            }
                    }
                })
                .AddTo(_disposables);
        }

        private void BindModelDataUpdate()
        {
            _gameModel.Score
                .DistinctUntilChanged()
                .Subscribe(score =>
                {
                    _gameUIView.UpdateScore(score);
                })
                .AddTo(_disposables);

            _gameModel.SkillPoint
                .DistinctUntilChanged()
                .Subscribe(skillPoint =>
                {
                    _gameUIView.UpdateSkillPoint(skillPoint, _gameData.MaxSkillPoint);
                    if (skillPoint == _gameData.MaxSkillPoint)
                    {
                        _seView.PlaySkillChargedSound();
                    }
                })
                .AddTo(_disposables);

            _gameModel.FeverPoint
                .DistinctUntilChanged()
                .Subscribe(feverPoint =>
                {
                    _gameUIView.UpdateFeverPoint(feverPoint, _gameData.MaxFeverPoint);
                })
                .AddTo(_disposables);

            _gameModel.IsSkillActivationReady
                .DistinctUntilChanged()
                .Subscribe(isReady =>
                {
                    _gameUIView.SetSkillCurtainActive(isReady);
                    if (isReady)
                    {
                        _gameUIView.PlaySkillInvocablePopEffect();
                    }
                })
                .AddTo(_disposables);
        }

        private void BindViewUpdate()
        {
            _readyAnimationEvent.OnReady
                .DistinctUntilChanged()
                .Subscribe(_ =>
                {
                    _gameModel.CurrentGameState.Value = GameModel.GameState.Playing;
                })
                .AddTo(_disposables);

            _inputEventHandler.OnInputStart
                .Where(_ =>
                {
                    return _gameModel.CurrentGameState.Value == GameModel.GameState.Playing;
                })
                .Subscribe(_ =>
                {
                    TsumView touchedTsum = _inputEventHandler.SelectTsum();

                    if (_gameModel.IsSkillActivationReady.Value)
                    {
                        if (touchedTsum != null)
                        {
                            _puzzleManager.ActivateWildcardSkill(touchedTsum);
                        }
                        return;
                    }

                    if (touchedTsum != null)
                    {
                        _puzzleManager.UpdateSelection(touchedTsum);
                    }
                })
                .AddTo(_disposables);

            _inputEventHandler.OnInputEnd
                .Where(_ =>
                {
                    return _gameModel.CurrentGameState.Value == GameModel.GameState.Playing;
                })
                .Subscribe(_ =>
                {
                    _puzzleManager.OnSelectionEnd();
                    _puzzleManager.UpdateSelectableHighlight();
                })
                .AddTo(_disposables);

            _gameUIView.OnSpawnButtonClicked
                .Where(_ =>
                {
                    return _gameModel.CurrentGameState.Value == GameModel.GameState.Playing;
                })
                .Subscribe(_ =>
                {
                    _seView.PlayDropSound();
                    int randomTsumId = _puzzleRule.GetRandomTsumID(_gameData.MaxSpawnTsumLevelIndex, _tsumData);
                    Vector2 spawnPosition = _tsumSpawner.GetRandomSpawnPosition();
                    _puzzleManager.CreateTsum(randomTsumId, spawnPosition);
                })
                .AddTo(_disposables);

            _gameUIView.OnSkillButtonClicked
                .Where(_ =>
                {
                    return _gameModel.CurrentGameState.Value == GameModel.GameState.Playing;
                })
                .Subscribe(_ =>
                {
                    if (_skillManager.TryActivateSkill())
                    {
                        _gameUIView.SetSkillCurtainActive(_gameModel.IsSkillActivationReady.Value);
                        _seView.PlaySkillActivatedSound();
                    }
                })
                .AddTo(_disposables);

            _gameUIView.OnTitleButtonClicked
                .Subscribe(async _ =>
                {
                    CrossfadeAudioController.ChangeClip(0);
                    await FadeMaskManager.FadeIn();
                    SceneManager.LoadScene("Title");
                })
                .AddTo(_disposables);

            _gameUIView.OnRetryButtonClicked
                .Subscribe(async _ =>
                {
                    await FadeMaskManager.FadeIn();
                    SceneManager.LoadScene("InGame");
                })
                .AddTo(_disposables);
        }

        private void SpawnInitialTsums()
        {
            for (int i = 0; i < _gameData.InitialTsumCount; i++)
            {
                int randomTsumId = _puzzleRule.GetRandomTsumID(_gameData.MaxSpawnTsumLevelIndex, _tsumData);
                Vector2 spawnPos = _tsumSpawner.GetRandomSpawnPosition();
                _puzzleManager.CreateTsum(randomTsumId, spawnPos);
            }
        }

        private void SelectTsum()
        {
            ITsumView selectedTsumView = _inputEventHandler.SelectTsum();

            if (selectedTsumView != null)
            {
                _puzzleManager.UpdateSelection(selectedTsumView);
            }
        }

        private void StartGame()
        {
            _gameModel.CurrentGameState.Value = GameModel.GameState.Ready;
        }
    }
}