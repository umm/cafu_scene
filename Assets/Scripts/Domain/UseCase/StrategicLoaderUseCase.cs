using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CAFU.Scene.Domain.Structure;
using UniRx;

namespace CAFU.Scene.Domain.UseCase
{
    public class StrategicLoaderUseCase : LoaderUseCaseBase
    {
        private IDictionary<string, int> ReferenceCounterMap { get; } = new Dictionary<string, int>();

        public override void Load(ISceneStrategy sceneStrategy)
        {
            if (!HasSceneStrategyStructure(sceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{sceneStrategy.SceneName}' in SceneStrategyMap.");
            }

            if (!sceneStrategy.CanLoadMultiple && HasLoaded(sceneStrategy))
            {
                return;
            }

            if (HasPreLoadSceneStrategyStructure(sceneStrategy.SceneName))
            {
                // First, load pre load scenes
                SceneStrategyMap[sceneStrategy.SceneName]
                    .PreLoadSceneNameList
                    .Select(x => SceneStrategyMap[x])
                    .Select(LoadAsObservable)
                    .WhenAll()
                    .ForEachAsync(
                        _ => SceneStrategyMap[sceneStrategy.SceneName]
                            .PreLoadSceneNameList
                            .ToList()
                            .ForEach(IncrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, load target scene
                    .SelectMany(_ => LoadAsObservable(sceneStrategy))
                    .Subscribe();
            }
            else
            {
                LoadAsObservable(sceneStrategy).Subscribe();
            }
        }

        public override void Unload(ISceneStrategy sceneStrategy)
        {
            if (!HasSceneStrategyStructure(sceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{sceneStrategy.SceneName}' in SceneStrategyMap.");
            }

            if (HasPostUnloadSceneStrategyStructure(sceneStrategy.SceneName))
            {
                // First, unload target scene
                UnloadAsObservable(sceneStrategy)
                    .ForEachAsync(
                        _ => SceneStrategyMap[sceneStrategy.SceneName]
                            .PostUnloadSceneNameList
                            .ToList()
                            .ForEach(DecrementReferenceCounter)
                    )
                    .ObserveOnMainThread()
                    // Second, unload post unload scenes
                    .SelectMany(
                        _ => SceneStrategyMap[sceneStrategy.SceneName]
                            .PostUnloadSceneNameList
                            .Where(CanUnloadScene)
                            .Select(x => SceneStrategyMap[x])
                            .Select(UnloadAsObservable)
                            .WhenAll()
                    )
                    .Subscribe();
            }
            else
            {
                UnloadAsObservable(sceneStrategy).Subscribe();
            }
        }

        protected override IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList()
        {
            return InitialSceneNameList.Select(x => SceneStrategyMap[x]);
        }

        protected override ISceneStrategy GetOrCreateSceneStrategy(string sceneName)
        {
            if (!HasSceneStrategyStructure(sceneName))
            {
                throw new KeyNotFoundException($"Does not find `{sceneName}' in SceneStrategyMap.");
            }
            return SceneStrategyMap[sceneName];
        }

        private bool HasSceneStrategyStructure(string sceneName)
        {
            return SceneStrategyMap.ContainsKey(sceneName);
        }

        private bool HasPreLoadSceneStrategyStructure(string sceneName)
        {
            return HasSceneStrategyStructure(sceneName) && SceneStrategyMap[sceneName].PreLoadSceneNameList.Any();
        }

        private bool HasPostUnloadSceneStrategyStructure(string sceneName)
        {
            return HasSceneStrategyStructure(sceneName) && SceneStrategyMap[sceneName].PostUnloadSceneNameList.Any();
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

        protected override void PreInitialize()
        {
            base.PreInitialize();
            ValidateSceneStrategyMap();
            ValidateInitialSceneNameList();
        }

        [Conditional("UNITY_EDITOR")]
        private void ValidateSceneStrategyMap()
        {
            foreach (var entry in SceneStrategyMap)
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

        [Conditional("UNITY_EDITOR")]
        private void ValidateInitialSceneNameList()
        {
            foreach (var sceneName in InitialSceneNameList)
            {
                if (!HasSceneStrategyStructure(sceneName))
                {
                    throw new InvalidOperationException($"SceneName `{sceneName}' does not determinate in SceneStrategyList.");
                }
            }
        }
    }
}