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
    [SerializeField] private PhysicsData _physicsData;

    [Header("View Components")]
    [SerializeField] private GameUIView _gameUIView;
    [SerializeField] private TsumSpawner _tsumSpawner;
    [SerializeField] private ReadyAnimationEvent _readyAnimationEvent;
    [SerializeField] private InputEventHandler _inputEventHandler;
    [SerializeField] private GameDebugView _gameDebugView;
    [SerializeField] private ChainLineHandler _chainLineHandler;
    [SerializeField] private GameOverZone _gameOverZone;
    [SerializeField] private PhysicsBoundary _physicsBoundary;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_gameData);
        builder.RegisterInstance(_tsumData);
        builder.RegisterInstance(_physicsData);

        builder.RegisterComponent(_gameUIView);

        builder.RegisterComponent(_readyAnimationEvent);
        builder.RegisterComponent(_inputEventHandler);
        builder.RegisterComponent(_physicsBoundary);

        builder.RegisterComponent(_tsumSpawner).AsImplementedInterfaces().AsSelf();
        builder.RegisterComponent(_chainLineHandler).AsImplementedInterfaces().AsSelf();

        if (_gameOverZone != null)
        {
            builder.RegisterComponent(_gameOverZone);
        }

        builder.Register<GameModel>(Lifetime.Singleton);

        builder.Register<PuzzleRule>(Lifetime.Singleton);

        builder.Register<TsumPhysicsManager>(Lifetime.Singleton).WithParameter(typeof(int), _gameData.MaxTsumCount);

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

        builder.RegisterEntryPoint<GamePresenter>(Lifetime.Singleton);
    }
}