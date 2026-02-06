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
using View.Logic;

namespace Presenter
{
    public class GamePresenter : IStartable, ITickable, IDisposable
    {
        private GameModel _gameModel;

        private PuzzleRule _puzzleRule;

        private ChainManager _chainManager;
        private TimeTsumSpawnManager _timeManager;
        private PuzzleManager _puzzleManager;

        private GameUIView _gameUIView;
        private TsumSpawner _tsumSpawner;
        private ReadyAnimationEvent _readyAnimationEvent;
        private InputEventHandler _inputEventHandler;
        private ChainLineHandler _chainLineHandler;

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
            GameUIView gameUIView,
            TsumSpawner tsumSpawner,
            InputEventHandler inputEventHandler,
            ReadyAnimationEvent readyAnimationEvent,
            ChainLineHandler chainLineHandler,
            GameOverZone gameOverZone,
            GameData gameData,
            TsumData tsumData
        )
        {
            _gameModel = gameModel;
            _chainManager = chainManager;
            _timeManager = timeManager;
            _puzzleManager = puzzleManager;
            _gameUIView = gameUIView;
            _tsumSpawner = tsumSpawner;
            _inputEventHandler = inputEventHandler;
            _readyAnimationEvent = readyAnimationEvent;
            _chainLineHandler = chainLineHandler;
            _gameOverZone = gameOverZone;
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
            BindModelUpdate();
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

            if (_puzzleManager.IsSelectionActive)
            {
                SelectTsum();
            }

            if (_timeManager.Tick(Time.deltaTime))
            {
                ITsum newTsum = _tsumSpawner.SpawnRandomFallingTsum(_gameData.MaxSpawnTsumLevelIndex);
                if (newTsum != null)
                {
                    _puzzleManager.RegisterTsum(newTsum);
                }

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

        private void BindModelUpdate()
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
                    Tsum firstTsum = _inputEventHandler.SelectTsum();
                    if (firstTsum != null && !firstTsum.IsDeleting)
                    {
                        _chainManager.AddTsumToChain(firstTsum);
                        firstTsum.OnSelected();
                        _puzzleManager.OnSelectionStart(firstTsum);
                        _puzzleManager.UpdateSelectableHighlight();
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
            if (_gameOverZone == null) return;

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
                ITsum newTsum = _tsumSpawner.SpawnRandomFallingTsum(_gameData.MaxSpawnTsumLevelIndex);
                if (newTsum != null)
                {
                    _puzzleManager.RegisterTsum(newTsum);
                }
            }
        }

        private void SelectTsum()
        {
            if (_chainManager.CurrentChain.Count == 0)
            {
                return;
            }

            Tsum selectedTsum = _inputEventHandler.SelectTsum();

            if (selectedTsum != null && selectedTsum.IsDeleting)
            {
                return;
            }

            if (selectedTsum != null && _puzzleManager.CurrentSelectingTsumID == selectedTsum.TsumID)
            {
                if (_chainManager.CurrentChain.Count >= 2)
                {
                    ITsum previousTsum = _chainManager.CurrentChain[^2];

                    if (previousTsum == (ITsum)selectedTsum)
                    {
                        ITsum tipTsum = _chainManager.CurrentChain.Last();
                        _chainManager.RemoveLastTsumFromChain();
                        _chainLineHandler.UpdateLine(_chainManager.CurrentChain);
                        tipTsum.OnUnselected();
                        return;
                    }
                }

                if (_chainManager.CurrentChain.Last() == (ITsum)selectedTsum)
                {
                    return;
                }

                if (_chainManager.CurrentChain.Contains((ITsum)selectedTsum))
                {
                    return;
                }

                if (_puzzleManager.CanConnectTsums(selectedTsum))
                {
                    _chainManager.AddTsumToChain(selectedTsum);
                    _chainLineHandler.UpdateLine(_chainManager.CurrentChain);
                    selectedTsum.OnSelected();
                }
            }
        }

        private void StartGame()
        {
            _gameModel.CurrentGameState.Value = GameModel.GameState.Ready;
        }
    }
}