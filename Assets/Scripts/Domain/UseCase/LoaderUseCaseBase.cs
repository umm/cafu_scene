using System;
using System.Collections.Generic;
using System.Linq;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public abstract class LoaderUseCaseBase : ILoaderUseCase, IInitializable
    {
        [Inject] private IFactory<ISceneStrategy, ISceneEntity> SceneEntityFactory { get; }

        [Inject] private IRequestEntity RequestEntity { get; }

        [Inject] private ISceneStateEntity SceneStateEntity { get; }

        [Inject] private ISceneRepository SceneRepository { get; }

        [Inject] private IRequestHandlerPresenter RequestHandlerPresenter { get; }

        [Inject(Id = Constant.InjectId.InitialSceneNameList)]
        protected IEnumerable<string> InitialSceneNameList { get; }

        [InjectOptional(Id = Constant.InjectId.UseCase.SceneStrategyMap)]
        protected IDictionary<string, ISceneStrategy> SceneStrategyMap { get; } = new Dictionary<string, ISceneStrategy>();

        private LinkedList<ISceneEntity> SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        void IInitializable.Initialize()
        {
            PreInitialize();
            RequestEntity.OnLoadRequestAsObservable().Subscribe(((ILoaderUseCase) this).Load);
            RequestEntity.OnUnloadRequestAsObservable().Subscribe(((ILoaderUseCase) this).Unload);
            RequestHandlerPresenter.RequestLoadSceneAsObservable().Select(GetOrCreateSceneStrategy).Subscribe(RequestEntity.RequestLoad);
            RequestHandlerPresenter.RequestUnloadSceneAsObservable().Select(GetOrCreateSceneStrategy).Subscribe(RequestEntity.RequestUnload);
            SceneManager.sceneLoaded += (scene, loadSceneMode) => SceneStateEntity.DidLoadSubject.OnNext(scene.name);
            SceneManager.sceneUnloaded += (scene) => SceneStateEntity.DidUnloadSubject.OnNext(scene.name);
            GenerateInitialSceneStrategyList().Select(SceneEntityFactory.Create).ToList().ForEach(x => SceneEntityList.AddLast(x));
            PostInitialize();
        }

        protected IObservable<Unit> LoadAsObservable(ISceneStrategy sceneStrategy)
        {
            // Do nothing if duplicate loading
            if (!sceneStrategy.CanLoadMultiple && RequestEntity.HasLoaded(sceneStrategy))
            {
                return Observable.ReturnUnit();
            }

            var sceneEntity = SceneEntityFactory.Create(sceneStrategy);
            SceneEntityList.AddLast(sceneEntity);
            SceneStateEntity.WillLoadSubject.OnNext(sceneEntity.SceneStrategy.SceneName);
            return SceneRepository
                .GetAsync(sceneStrategy)
                .ToObservable()
                .SelectMany(
                    _ => sceneEntity
                        .Load()
                        .ToObservable()
                );
        }

        protected IObservable<Unit> UnloadAsObservable(ISceneStrategy sceneStrategy)
        {
            if (SceneEntityList.All(x => x.SceneStrategy.SceneName != sceneStrategy.SceneName))
            {
                return Observable.Throw<Unit>(new ArgumentOutOfRangeException($"Scene name `{sceneStrategy.SceneName}' does not contain in loaded scenes."));
            }

            SceneStateEntity.WillUnloadSubject.OnNext(sceneStrategy.SceneName);
            // Search from last
            var sceneEntity = SceneEntityList.Last(x => x.SceneStrategy.SceneName == sceneStrategy.SceneName);
            return sceneEntity
                .Unload()
                .ToObservable()
                .ForEachAsync(_ => SceneEntityList.Remove(sceneEntity));
        }

        protected bool HasLoaded(ISceneStrategy sceneStrategy)
        {
            return RequestEntity.HasLoaded(sceneStrategy);
        }

        public abstract void Load(ISceneStrategy sceneStrategy);
        public abstract void Unload(ISceneStrategy sceneStrategy);
        protected abstract IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList();
        protected abstract ISceneStrategy GetOrCreateSceneStrategy(string sceneName);

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void PreInitialize()
        {
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void PostInitialize()
        {
        }
    }
}