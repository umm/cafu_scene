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
        [Inject] private IFactory<ISceneStrategy, ISceneEntity> SceneEntityFactory { get; set; }
        [Inject] private IRequestEntity RequestEntity { get; set; }
        [Inject] private ISceneStateEntity SceneStateEntity { get; set; }
        [Inject] private ISceneRepository SceneRepository { get; set; }
        [Inject] private IRequestHandlerPresenter RequestHandlerPresenter { get; set; }
        [Inject(Id = Constant.InjectId.InitialSceneNameList)]
        protected IEnumerable<string> InitialSceneNameList { get; set; }
        [InjectOptional(Id = Constant.InjectId.UseCase.SceneStrategyMap)]
        protected IDictionary<string, ISceneStrategy> SceneStrategyMap { get; set; } = new Dictionary<string, ISceneStrategy>();
        private LinkedList<ISceneEntity> SceneEntityList { get; } = new LinkedList<ISceneEntity>();
        private IDictionary<string, IDisposable> LoadDisposableMap { get; } = new Dictionary<string, IDisposable>();

        void IInitializable.Initialize()
        {
            PreInitialize();
            RequestEntity.OnLoadRequestAsObservable().Select(GetOrCreateSceneStrategy).Subscribe(((ILoaderUseCase)this).Load);
            RequestEntity.OnUnloadRequestAsObservable().Select(GetOrCreateSceneStrategy).Subscribe(((ILoaderUseCase)this).Unload);
            RequestHandlerPresenter.RequestLoadSceneAsObservable().Subscribe(RequestEntity.RequestLoad);
            RequestHandlerPresenter.RequestUnloadSceneAsObservable().Subscribe(RequestEntity.RequestUnload);
            SceneManager.sceneLoaded += (scene, loadSceneMode) => SceneStateEntity.DidLoadSubject.OnNext(scene.name);
            SceneManager.sceneUnloaded += (scene) => SceneStateEntity.DidUnloadSubject.OnNext(scene.name);
            GenerateInitialSceneStrategyList().Select(SceneEntityFactory.Create).ToList().ForEach(x => SceneEntityList.AddLast(x));
            PostInitialize();
        }

        protected IObservable<Unit> LoadAsObservable(ISceneStrategy sceneStrategy)
        {
            // Do nothing if duplicate loading
            if (!CanLoadScene(sceneStrategy))
            {
                return Observable.ReturnUnit();
            }

            var sceneEntity = SceneEntityFactory.Create(sceneStrategy);
            // For blocking when a Load instruction flew in the same frame
            LoadDisposableMap[sceneStrategy.SceneName] = sceneEntity.DidLoadAsObservable().Take(1).Subscribe();
            SceneEntityList.AddLast(sceneEntity);
            SceneStateEntity.WillLoadSubject.OnNext(sceneEntity.SceneStrategy.SceneName);
            return SceneRepository
                .GetAsync(sceneStrategy)
                .ToObservable()
                .SelectMany(
                    _ => sceneEntity
                        .Load()
                        .ToObservable()
                )
                .ForEachAsync(_ => LoadDisposableMap.Remove(sceneStrategy.SceneName));
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

        protected bool CanLoadScene(ISceneStrategy sceneStrategy)
        {
            if (sceneStrategy.CanLoadMultiple)
            {
                return true;
            }

            return !HasLoaded(sceneStrategy) && (!LoadDisposableMap.ContainsKey(sceneStrategy.SceneName) || LoadDisposableMap[sceneStrategy.SceneName] == null);
        }

        private bool HasLoaded(ISceneStrategy sceneStrategy)
        {
            return RequestEntity.HasLoaded(sceneStrategy.SceneName);
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