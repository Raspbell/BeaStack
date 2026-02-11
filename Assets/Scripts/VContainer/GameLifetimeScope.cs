using VContainer;
using VContainer.Unity;
using UnityEngine;

using Model;
using Model.Logic;
using Model.Data;
using View;
using Presenter;

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
    [SerializeField] private GameOverZone _gameOverZone;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_gameData);
        builder.RegisterInstance(_tsumData);

        builder.RegisterComponent(_gameUIView);

        builder.RegisterComponent(_readyAnimationEvent);
        builder.RegisterComponent(_inputEventHandler);

        // インターフェースとして登録
        builder.RegisterComponent(_tsumSpawner).AsImplementedInterfaces().AsSelf();
        builder.RegisterComponent(_chainLineHandler).AsImplementedInterfaces().AsSelf();

        if (_gameOverZone != null)
        {
            builder.RegisterComponent(_gameOverZone);
        }

        // Model
        builder.Register<GameModel>(Lifetime.Singleton);

        // Logic (Service)
        builder.Register<PuzzleRule>(Lifetime.Singleton);

        // Manager
        builder.Register<ChainManager>(Lifetime.Singleton);
        builder.Register<TimeTsumSpawnManager>(Lifetime.Singleton);
        builder.Register<PuzzleManager>(Lifetime.Singleton);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_gameDebugView != null)
        {
            builder.RegisterComponent(_gameDebugView);
            builder.RegisterEntryPoint<GameDebugPresenter>(Lifetime.Singleton);
        }
#endif

        // Presenter
        builder.RegisterEntryPoint<GamePresenter>(Lifetime.Singleton);
    }
}