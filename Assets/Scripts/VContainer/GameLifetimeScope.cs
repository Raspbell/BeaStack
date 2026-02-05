using VContainer;
using VContainer.Unity;
using UnityEngine;

// GameInitializer の代わりになる「設定ファイル」
public class GameLifetimeScope : LifetimeScope
{
    [Header("Data")]
    [SerializeField] private GameData _gameData;
    [SerializeField] private TsumData _tsumData;

    [Header("View Components")]
    [SerializeField] private GameUIView _gameUIView;
    [SerializeField] private TsumSpawner _tsumSpawner;
    [SerializeField] private ReadyAnimationEvent _readyAnimationEvent;
    [SerializeField] private InputEventHandler _inputEventHandler;
    [SerializeField] private GameDebugView _gameDebugView;
    [SerializeField] private ChainLineHandler _chainLineHandler;

    protected override void Configure(IContainerBuilder builder)
    {
        // 1. データの登録 (Instanceとしてそのまま渡す)
        builder.RegisterInstance(_gameData);
        builder.RegisterInstance(_tsumData);

        // 2. Component (View) の登録
        builder.RegisterComponent(_gameUIView);
        builder.RegisterComponent(_tsumSpawner);
        builder.RegisterComponent(_readyAnimationEvent);
        builder.RegisterComponent(_inputEventHandler);
        builder.RegisterComponent(_chainLineHandler);

        // 3. Logic / Model の登録 (Pure C#クラス)

        // Model
        builder.Register<GameModel>(Lifetime.Singleton);

        // Logic (Service)
        builder.Register<PuzzleRule>(Lifetime.Singleton);

        // Manager
        builder.Register<ChainManager>(Lifetime.Singleton);
        builder.Register<TimeManager>(Lifetime.Singleton);
        builder.Register<PuzzleManager>(Lifetime.Singleton);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_gameDebugView != null)
        {
            builder.RegisterComponent(_gameDebugView);
            builder.RegisterEntryPoint<GameDebugPresenter>(Lifetime.Singleton);
        }
#endif

        // 4. Presenter (EntryPoint) の登録
        // GamePresenter は「起動役」なので EntryPoint として登録する
        builder.RegisterEntryPoint<GamePresenter>(Lifetime.Singleton);
    }
}