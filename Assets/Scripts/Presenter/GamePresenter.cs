using UnityEngine;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System.Linq;

using Model;
using Model.Logic;
using Model.Interface;
using Model.Data;
using View;

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

        private GameUIView _gameUIView;
        private TsumSpawner _tsumSpawner;
        private ReadyAnimationEvent _readyAnimationEvent;
        private InputEventHandler _inputEventHandler;
        private ChainLineHandler _chainLineHandler;
        private PhysicsBoundary _physicsBoundary;
        private CameraAspectHandler _cameraAspectHandler;
        private ScreenDisplayArea _screenDisplayArea;

        private GameOverZone _gameOverZone;

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
            PuzzleRule puzzleRule,
            GameUIView gameUIView,
            TsumSpawner tsumSpawner,
            InputEventHandler inputEventHandler,
            ReadyAnimationEvent readyAnimationEvent,
            ChainLineHandler chainLineHandler,
            GameOverZone gameOverZone,
            PhysicsBoundary physicsBoundary,
            GameData gameData,
            TsumData tsumData
        )
        {
            _gameModel = gameModel;
            _chainManager = chainManager;
            _timeManager = timeManager;
            _puzzleManager = puzzleManager;
            _tsumPhysicsManager = tsumPhysicsManager;
            _puzzleRule = puzzleRule;
            _gameUIView = gameUIView;
            _tsumSpawner = tsumSpawner;
            _inputEventHandler = inputEventHandler;
            _readyAnimationEvent = readyAnimationEvent;
            _chainLineHandler = chainLineHandler;
            _gameOverZone = gameOverZone;
            _physicsBoundary = physicsBoundary;
            _gameData = gameData;
            _tsumData = tsumData;
        }

        public void Start()
        {
            _tsumSpawner.Initialize(_tsumData, _gameUIView);
            _timeManager.Initialize();

            if (_gameOverZone != null)
            {
                _gameOverZone.Initialize(_gameData.GameOverGraceTime);
            }

            BindGameState();
            BindModelDataUpdate();
            BindViewUpdate();
            BindGameOverZone();

            StartGame();
        }

        public void Tick()
        {
            if (_gameModel.CurrentGameState.Value != GameModel.GameState.Playing)
            {
                return;
            }

            float alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            foreach (var tsum in _puzzleManager.AllTsums)
            {
                // Vector2 tsumPosition = _tsumPhysicsManager.GetTsumPosition(tsum.PhysicsIndex);
                // tsum.TsumView.UpdatePosition(new Vector3(tsumPosition.x, tsumPosition.y));

                Vector2 interpolatedPos = _tsumPhysicsManager.GetInterpolatedPosition(tsum.PhysicsIndex, alpha);
                tsum.TsumView.UpdatePosition(interpolatedPos);
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

            _tsumPhysicsManager.UpdateAllTsumPosition(Time.fixedDeltaTime, _physicsBoundary.LeftX, _physicsBoundary.RightX, _physicsBoundary.BottomY, _physicsBoundary.TopY);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void BindGameState()
        {
            _gameModel.CurrentGameState
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    switch (state)
                    {
                        case GameModel.GameState.Ready:
                            _gameUIView.PlayReadyAnimation();
                            break;
                        case GameModel.GameState.Playing:
                            SpawnInitialTsums();
                            break;
                        case GameModel.GameState.GameOver:
                            Debug.Log("Game Over!");
                            break;
                    }
                })
                .AddTo(_disposables);
        }

        private void BindModelDataUpdate()
        {
            _gameModel.Score
                .DistinctUntilChanged()
                .Subscribe(score => _gameUIView.UpdateScore(score))
                .AddTo(_disposables);

            _gameModel.SkillPoint
                .DistinctUntilChanged()
                .Subscribe(skillPoint => _gameUIView.UpdateSkillPoint(skillPoint, _gameData.MaxSkillPoint))
                .AddTo(_disposables);

            _gameModel.FeverPoint
                .DistinctUntilChanged()
                .Subscribe(feverPoint => _gameUIView.UpdateFeverPoint(feverPoint, _gameData.MaxFeverPoint))
                .AddTo(_disposables);
        }

        private void BindViewUpdate()
        {
            _readyAnimationEvent.OnReady
                .DistinctUntilChanged()
                .Subscribe(_ => _gameModel.CurrentGameState.Value = GameModel.GameState.Playing)
                .AddTo(_disposables);

            _inputEventHandler.OnInputStart
                .Where(_ => _gameModel.CurrentGameState.Value == GameModel.GameState.Playing)
                .Subscribe(_ =>
                {
                    TsumView firstTsumView = _inputEventHandler.SelectTsum();
                    if (firstTsumView != null)
                    {
                        _puzzleManager.UpdateSelection(firstTsumView);
                    }
                })
                .AddTo(_disposables);

            _inputEventHandler.OnInputEnd
                .Where(_ => _gameModel.CurrentGameState.Value == GameModel.GameState.Playing)
                .Subscribe(_ =>
                {
                    _puzzleManager.OnSelectionEnd();
                    _puzzleManager.UpdateSelectableHighlight();
                })
                .AddTo(_disposables);

        }

        private void BindGameOverZone()
        {
            if (_gameOverZone == null)
            {
                return;
            }

            _gameOverZone.OnGameOver
                .Where(_ => _gameModel.CurrentGameState.Value == GameModel.GameState.Playing)
                .Subscribe(_ =>
                {
                    _gameModel.CurrentGameState.Value = GameModel.GameState.GameOver;
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