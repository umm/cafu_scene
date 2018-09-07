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
        [Inject] IFactory<string, ISceneEntity> ILoaderUseCase.SceneEntityFactory { get; }

        [Inject] ILoadRequestEntity ILoaderUseCase.LoadRequestEntity { get; }

        [Inject] ISceneStateEntity ILoaderUseCase.SceneStateEntity { get; }

        [Inject] ISceneRepository ILoaderUseCase.SceneRepository { get; }

        LinkedList<ISceneEntity> ILoaderUseCase.SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        [Inject(Id = Constant.InjectId.InitialSceneNameList)] IEnumerable<string> ILoaderUseCase.InitialSceneNameList { get; }

        [Inject(Id = Constant.InjectId.UseCase.SceneStrategyMap)]
        private IReadOnlyDictionary<string, ISceneStrategyStructure> SceneStrategyStructureMap { get; } = new Dictionary<string, ISceneStrategyStructure>();

        private IDictionary<string, int> ReferenceCounterMap { get; } = new Dictionary<string, int>();

        void IInitializable.Initialize()
        {
            this.InitializeUseCase();
            Validate();
            // Increment reference counters of scenes referenced by initial scenes
            ((ILoaderUseCase) this).InitialSceneNameList
                .SelectMany(x => SceneStrategyStructureMap[x].PreLoadSceneNameList)
                .ToList()
                .ForEach(IncrementReferenceCounter);
        }

        public void Load(ILoadRequestStructure loadRequestStructure)
        {
            if (!HasSceneStrategyStructure(loadRequestStructure.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{loadRequestStructure.SceneName}' in SceneStrategyStructureMap.");
            }

            if (HasPreLoadSceneStrategyStructure(loadRequestStructure.SceneName))
            {
                // First, load target scene
                SceneStrategyStructureMap[loadRequestStructure.SceneName]
                    .PreLoadSceneNameList
                    .Select(x => SceneStrategyStructureMap[x])
                    .Select(x => this.LoadAsObservable(x.SceneName, x.LoadAsSingle, x.CanLoadMultiple))
                    .WhenAll()
                    .ForEachAsync(
                        _ => SceneStrategyStructureMap[loadRequestStructure.SceneName]
                            .PreLoadSceneNameList
                            .ToList()
                            .ForEach(IncrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, load pre load scenes
                    .SelectMany(_ => this.LoadAsObservable(loadRequestStructure.SceneName, loadRequestStructure.LoadAsSingle, loadRequestStructure.CanLoadMultiple))
                    .Subscribe();
            }
            else
            {
                this.LoadAsObservable(loadRequestStructure.SceneName, loadRequestStructure.LoadAsSingle, loadRequestStructure.CanLoadMultiple).Subscribe();
            }
        }

        public void Unload(IUnloadRequestStructure unloadRequestStructure)
        {
            if (!HasSceneStrategyStructure(unloadRequestStructure.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{unloadRequestStructure.SceneName}' in SceneStrategyStructureMap.");
            }

            if (HasPostUnloadSceneStrategyStructure(unloadRequestStructure.SceneName))
            {
                // First, unload target scene
                this.UnloadAsObservable(unloadRequestStructure.SceneName)
                    .ForEachAsync(
                        _ => SceneStrategyStructureMap[unloadRequestStructure.SceneName]
                            .PostUnloadSceneNameList
                            .ToList()
                            .ForEach(DecrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, unload post unload scenes
                    .SelectMany(
                        _ => SceneStrategyStructureMap[unloadRequestStructure.SceneName]
                            .PostUnloadSceneNameList
                            .Where(CanUnloadScene)
                            .Select(x => SceneStrategyStructureMap[x])
                            .Select(x => this.UnloadAsObservable(x.SceneName))
                            .WhenAll()
                    )
                    .Subscribe();
            }
            else
            {
                this.UnloadAsObservable(unloadRequestStructure.SceneName).Subscribe();
            }
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