using UnityEngine;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class GamePresenter : IStartable, ITickable, IDisposable
{
    private GameModel _gameModel;
    private PuzzleRule _puzzleRule;

    private ChainManager _chainManager;
    private TimeManager _timeManager;
    private PuzzleManager _puzzleManager;

    private GameUIView _gameUIView;
    private TsumSpawner _tsumSpawner;
    private ReadyAnimationEvent _readyAnimationEvent;
    private InputEventHandler _inputEventHandler;
    private ChainLineHandler _chainLineHandler;

    private GameData _gameData;
    private TsumData _tsumData;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    [Inject]
    public GamePresenter(
        GameModel gameModel,
        ChainManager chainManager,
        TimeManager timeManager,
        PuzzleManager puzzleManager,
        GameUIView gameUIView,
        TsumSpawner tsumSpawner,
        InputEventHandler inputEventHandler,
        ReadyAnimationEvent readyAnimationEvent,
        ChainLineHandler chainLineHandler,
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
        _gameData = gameData;
        _tsumData = tsumData;
    }

    public void Start()
    {
        _tsumSpawner.Initialize(_tsumData, _gameUIView);

        BindGameState();
        BindModelUpdate();
        BindViewUpdate();

        StartGame();
    }

    public void Tick()
    {
        _timeManager.TimerTick(Time.deltaTime);

        if (_puzzleManager.IsSelectionActive)
        {
            SelectTsum();
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

        _gameModel.TimeRemaining
            .DistinctUntilChanged()
            .Subscribe(timeRemaining => _gameUIView.UpdateTimer(timeRemaining, _gameData.MaxTime))
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

    private void SpawnInitialTsums()
    {
        for (int i = 0; i < _gameData.MaxTsumNum; i++)
        {
            int randomIdx = UnityEngine.Random.Range(0, _tsumData.TsumEntities.Length);
            int tsumId = _tsumData.TsumEntities[randomIdx].TsumID;
            Tsum newTsum = _tsumSpawner.SpawnTsum(tsumId);
            _puzzleManager.RegisterTsum(newTsum);
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
                Tsum previousTsum = _chainManager.CurrentChain[^2];
                if (previousTsum == selectedTsum)
                {
                    Tsum tipTsum = _chainManager.CurrentChain.Last();
                    _chainManager.RemoveLastTsumFromChain();
                    _chainLineHandler.UpdateLine(_chainManager.CurrentChain);
                    tipTsum.OnUnselected();
                    return;
                }
            }

            if (_chainManager.CurrentChain.Last() == selectedTsum)
            {
                return;
            }

            if (_chainManager.CurrentChain.Contains(selectedTsum))
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