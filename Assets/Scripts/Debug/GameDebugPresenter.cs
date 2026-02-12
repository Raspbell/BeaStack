using VContainer;
using VContainer.Unity;
using UniRx;
using UnityEngine.SceneManagement;
using System;
using Model;
using Model.Data;
using Model.Logic;

namespace GameDebug
{
    public class GameDebugPresenter : IStartable, ITickable, IDisposable
    {
        private readonly GameDebugView _view;
        private readonly GameModel _model;

        private readonly GameoverManager _gameoverManager;
        private readonly TsumPhysicsManager _physicsManager;
        private readonly GameData _gameData;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public GameDebugPresenter(
            GameDebugView view,
            GameModel model,
            GameoverManager gameoverManager,
            TsumPhysicsManager physicsManager,
            GameData gameData
        )
        {
            _view = view;
            _model = model;
            _gameoverManager = gameoverManager;
            _physicsManager = physicsManager;
            _gameData = gameData;
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