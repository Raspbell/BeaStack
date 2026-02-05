using VContainer;
using VContainer.Unity;
using UniRx;
using UnityEngine.SceneManagement;
using System;

public class GameDebugPresenter : IStartable, IDisposable
{
    private readonly GameDebugView _view;
    private readonly GameModel _model;

    // private readonly TimeManager _timeManager; 

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    [Inject]
    public GameDebugPresenter(
        GameDebugView view,
        GameModel model
    )
    {
        _view = view;
        _model = model;
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

    public void Dispose()
    {
        _disposables.Dispose();
    }
}