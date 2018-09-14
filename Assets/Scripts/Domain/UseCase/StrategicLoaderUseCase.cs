using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using UniRx;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public class StrategicLoaderUseCase : ILoaderUseCase, IInitializable
    {
        [Inject] IFactory<ISceneStrategy, ISceneEntity> ILoaderUseCase.SceneEntityFactory { get; }

        [Inject] ILoadRequestEntity ILoaderUseCase.LoadRequestEntity { get; }

        [Inject] ISceneStateEntity ILoaderUseCase.SceneStateEntity { get; }

        [Inject] ISceneRepository ILoaderUseCase.SceneRepository { get; }

        LinkedList<ISceneEntity> ILoaderUseCase.SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        [Inject(Id = Constant.InjectId.InitialSceneNameList)]
        private IEnumerable<string> InitialSceneNameList { get; }

        [Inject(Id = Constant.InjectId.UseCase.SceneStrategyMap)]
        private IReadOnlyDictionary<string, ISceneStrategy> SceneStrategyStructureMap { get; } = new Dictionary<string, ISceneStrategy>();

        private IDictionary<string, int> ReferenceCounterMap { get; } = new Dictionary<string, int>();

        void IInitializable.Initialize()
        {
            this.InitializeUseCase();
            Validate();
            // Increment reference counters of scenes referenced by initial scenes
            InitialSceneNameList
                .SelectMany(x => SceneStrategyStructureMap[x].PreLoadSceneNameList)
                .ToList()
                .ForEach(IncrementReferenceCounter);
            ((ILoaderUseCase) this).LoadRequestEntity.SetSceneStrategyStructureResolver(x => SceneStrategyStructureMap[x]);
        }

        public void Load(ILoadRequest loadRequest)
        {
            if (!HasSceneStrategyStructure(loadRequest.SceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{loadRequest.SceneStrategy.SceneName}' in SceneStrategyStructureMap.");
            }

            if (!loadRequest.SceneStrategy.CanLoadMultiple && ((ILoaderUseCase) this).LoadRequestEntity.HasLoaded(loadRequest.SceneStrategy.SceneName))
            {
                return;
            }

            if (HasPreLoadSceneStrategyStructure(loadRequest.SceneStrategy.SceneName))
            {
                // First, load target scene
                SceneStrategyStructureMap[loadRequest.SceneStrategy.SceneName]
                    .PreLoadSceneNameList
                    .Select(x => SceneStrategyStructureMap[x])
                    .Select(this.LoadAsObservable)
                    .WhenAll()
                    .ForEachAsync(
                        _ => SceneStrategyStructureMap[loadRequest.SceneStrategy.SceneName]
                            .PreLoadSceneNameList
                            .ToList()
                            .ForEach(IncrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, load pre load scenes
                    .SelectMany(_ => this.LoadAsObservable(loadRequest.SceneStrategy))
                    .Subscribe();
            }
            else
            {
                this.LoadAsObservable(loadRequest.SceneStrategy).Subscribe();
            }
        }

        public void Unload(IUnloadRequest unloadRequest)
        {
            if (!HasSceneStrategyStructure(unloadRequest.SceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{unloadRequest.SceneStrategy.SceneName}' in SceneStrategyStructureMap.");
            }

            if (HasPostUnloadSceneStrategyStructure(unloadRequest.SceneStrategy.SceneName))
            {
                // First, unload target scene
                this.UnloadAsObservable(unloadRequest.SceneStrategy)
                    .ForEachAsync(
                        _ => SceneStrategyStructureMap[unloadRequest.SceneStrategy.SceneName]
                            .PostUnloadSceneNameList
                            .ToList()
                            .ForEach(DecrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, unload post unload scenes
                    .SelectMany(
                        _ => SceneStrategyStructureMap[unloadRequest.SceneStrategy.SceneName]
                            .PostUnloadSceneNameList
                            .Where(CanUnloadScene)
                            .Select(x => SceneStrategyStructureMap[x])
                            .Select(this.UnloadAsObservable)
                            .WhenAll()
                    )
                    .Subscribe();
            }
            else
            {
                this.UnloadAsObservable(unloadRequest.SceneStrategy).Subscribe();
            }
        }

        public IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList()
        {
            return InitialSceneNameList.Select(x => SceneStrategyStructureMap[x]);
        }

        private bool HasSceneStrategyStructure(string sceneName)
        {
            return SceneStrategyStructureMap.ContainsKey(sceneName);
        }

        private bool HasPreLoadSceneStrategyStructure(string sceneName)
        {
            return HasSceneStrategyStructure(sceneName) && SceneStrategyStructureMap[sceneName].PreLoadSceneNameList.Any();
        }

        private bool HasPostUnloadSceneStrategyStructure(string sceneName)
        {
            return HasSceneStrategyStructure(sceneName) && SceneStrategyStructureMap[sceneName].PostUnloadSceneNameList.Any();
        }

        private bool CanUnloadScene(string sceneName)
        {
            return !ReferenceCounterMap.ContainsKey(sceneName) || ReferenceCounterMap[sceneName] <= 0;
        }

        private void IncrementReferenceCounter(string sceneName)
        {
            if (!ReferenceCounterMap.ContainsKey(sceneName))
            {
                ReferenceCounterMap[sceneName] = 0;
            }

            ReferenceCounterMap[sceneName]++;
        }

        private void DecrementReferenceCounter(string sceneName)
        {
            if (!ReferenceCounterMap.ContainsKey(sceneName))
            {
                ReferenceCounterMap[sceneName] = 0;
            }
            else if (ReferenceCounterMap[sceneName] <= 0)
            {
                ReferenceCounterMap[sceneName] = 0;
            }
            else
            {
                ReferenceCounterMap[sceneName]--;
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void Validate()
        {
            foreach (var entry in SceneStrategyStructureMap)
            {
                foreach (var preLoadSceneName in entry.Value.PreLoadSceneNameList)
                {
                    if (!HasSceneStrategyStructure(preLoadSceneName))
                    {
                        throw new InvalidOperationException($"SceneName `{preLoadSceneName}' specified as pre load in strategy of `{entry.Key}'.");
                    }
                }

                foreach (var postUnloadSceneName in entry.Value.PostUnloadSceneNameList)
                {
                    if (!HasSceneStrategyStructure(postUnloadSceneName))
                    {
                        throw new InvalidOperationException($"SceneName `{postUnloadSceneName}' specified as post unload in strategy of `{entry.Key}'.");
                    }
                }
            }
        }
    }
}