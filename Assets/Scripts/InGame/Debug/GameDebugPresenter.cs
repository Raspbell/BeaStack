using VContainer;
using VContainer.Unity;
using UniRx;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using Model;
using Model.Data;
using Model.Logic;
using Model.Interface;

namespace GameDebug
{
    public class GameDebugPresenter : IStartable, ITickable, IDisposable
    {
        private readonly GameDebugView _view;
        private readonly GameModel _model;

        private readonly GameoverManager _gameoverManager;
        private readonly TsumPhysicsManager _physicsManager;
        private readonly GameData _gameData;
        private readonly PuzzleManager _puzzleManager;
        private readonly ITsumSpawner _tsumSpawner;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public GameDebugPresenter(
            GameDebugView view,
            GameModel model,
            GameoverManager gameoverManager,
            TsumPhysicsManager physicsManager,
            GameData gameData,
            PuzzleManager puzzleManager,
            ITsumSpawner tsumSpawner
        )
        {
            _view = view;
            _model = model;
            _gameoverManager = gameoverManager;
            _physicsManager = physicsManager;
            _gameData = gameData;
            _puzzleManager = puzzleManager;
            _tsumSpawner = tsumSpawner;
        }

        public void Start()
        {
            // リセット
            _view.OnResetRequest
                .Subscribe(_ =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                })
                .AddTo(_disposables);

            // スコア加算
            _view.OnAddScoreRequest
                .Subscribe(_ =>
                {
                    _model.Score.Value += 10000;
                })
                .AddTo(_disposables);

            // 時間延長
            _view.OnTimeExtensionRequest
                .Subscribe(_ =>
                {
                    _model.TimeRemaining.Value += 60f;
                })
                .AddTo(_disposables);

            // フィーバーテスト
            _view.OnFeverRequest
                .Subscribe(_ =>
                {
                    _model.IsFever.Value = true;
                    _model.FeverPoint.Value = 100;
                })
                .AddTo(_disposables);

            // 指定IDのツム生成
            _view.OnSpawnTsumRequest
                .Subscribe(tsumId =>
                {
                    if (_model.CurrentGameState.Value == GameModel.GameState.Playing)
                    {
                        Vector2 spawnPos = _tsumSpawner.GetRandomSpawnPosition();
                        _puzzleManager.CreateTsum(tsumId, spawnPos);
                    }
                })
                .AddTo(_disposables);
        }

        public void Tick()
        {
            if (_view == null)
            {
                return;
            }

            // データを取得してViewへ流す
            float currentGrace = _gameoverManager.CurrentGraceTime;
            float maxGrace = _gameData.GameOverGraceTime;
            int targetCount = _physicsManager.GetGameoverTargetCount();

            _view.UpdateGameoverInfo(currentGrace, maxGrace, targetCount);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}