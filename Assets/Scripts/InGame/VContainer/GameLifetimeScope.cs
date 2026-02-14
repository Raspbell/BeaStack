using VContainer;
using VContainer.Unity;
using UnityEngine;

using InGame.Model;
using InGame.Model.Logic;
using InGame.Model.Data;
using InGame.View;
using Presenter;

using InGame.GameDebug.Presenter;
using InGame.GameDebug.View;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Data")]
    [SerializeField] private GameData _gameData;
    [SerializeField] private TsumData _tsumData;
    [SerializeField] private PhysicsData _physicsData;

    [Header("View")]
    [SerializeField] private GameUIView _gameUIView;
    [SerializeField] private TsumSpawner _tsumSpawner;
    [SerializeField] private ReadyAnimationEvent _readyAnimationEvent;
    [SerializeField] private InputEventHandler _inputEventHandler;
    [SerializeField] private GameDebugView _gameDebugView;
    [SerializeField] private ParticleSpawner _particleSpawner;
    [SerializeField] private ChainLineHandler _chainLineHandler;
    [SerializeField] private PhysicsBoundary _physicsBoundary;
    [SerializeField] private SEView _seView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_gameData);
        builder.RegisterInstance(_tsumData);
        builder.RegisterInstance(_physicsData);

        builder.RegisterComponent(_gameUIView);

        builder.RegisterComponent(_readyAnimationEvent);
        builder.RegisterComponent(_inputEventHandler);
        builder.RegisterComponent(_physicsBoundary);
        builder.RegisterComponent(_particleSpawner);
        builder.RegisterComponent(_tsumSpawner).AsImplementedInterfaces().AsSelf();
        builder.RegisterComponent(_chainLineHandler).AsImplementedInterfaces().AsSelf();
        builder.RegisterComponent(_seView).AsImplementedInterfaces().AsSelf();

        builder.Register<GameModel>(Lifetime.Singleton);

        builder.Register<PuzzleRule>(Lifetime.Singleton);

        builder.Register<TsumPhysicsManager>(Lifetime.Singleton).WithParameter(typeof(int), _gameData.MaxTsumCount);

        builder.Register<ChainManager>(Lifetime.Singleton);
        builder.Register<TimeTsumSpawnManager>(Lifetime.Singleton);
        builder.Register<PuzzleManager>(Lifetime.Singleton);
        builder.Register<GameoverManager>(Lifetime.Singleton);
        builder.Register<SkillManager>(Lifetime.Singleton);

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