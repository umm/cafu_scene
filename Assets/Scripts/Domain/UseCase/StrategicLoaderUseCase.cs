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
            if (!HasSceneStrategy(sceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{sceneStrategy.SceneName}' in SceneStrategyMap.");
            }

            if (!CanLoadScene(sceneStrategy))
            {
                return;
            }

            if (HasPreLoadSceneStrategy(sceneStrategy.SceneName))
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
            if (!HasSceneStrategy(sceneStrategy.SceneName))
            {
                throw new ArgumentOutOfRangeException($"Does not find `{sceneStrategy.SceneName}' in SceneStrategyMap.");
            }

            if (HasPostUnloadSceneStrategy(sceneStrategy.SceneName))
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
                            .Where(x => !ReferenceCounterMap.ContainsKey(x) || ReferenceCounterMap[x] <= 0)
                            .Select(x => SceneStrategyMap[x])
                            .Where(x => !x.ProtectFromUnloading)
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
            if (!HasSceneStrategy(sceneName))
            {
                throw new KeyNotFoundException($"Does not find `{sceneName}' in SceneStrategyMap.");
            }
            return SceneStrategyMap[sceneName];
        }

        private bool HasSceneStrategy(string sceneName)
        {
            return SceneStrategyMap.ContainsKey(sceneName);
        }

        private bool HasPreLoadSceneStrategy(string sceneName)
        {
            return HasSceneStrategy(sceneName) && SceneStrategyMap[sceneName].PreLoadSceneNameList.Any();
        }

        private bool HasPostUnloadSceneStrategy(string sceneName)
        {
            return HasSceneStrategy(sceneName) && SceneStrategyMap[sceneName].PostUnloadSceneNameList.Any();
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
                    if (!HasSceneStrategy(preLoadSceneName))
                    {
                        throw new InvalidOperationException($"SceneName `{preLoadSceneName}' specified as pre load in strategy of `{entry.Key}'.");
                    }
                }

                foreach (var postUnloadSceneName in entry.Value.PostUnloadSceneNameList)
                {
                    if (!HasSceneStrategy(postUnloadSceneName))
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
                if (!HasSceneStrategy(sceneName))
                {
                    throw new InvalidOperationException($"SceneName `{sceneName}' does not determinate in SceneStrategyList.");
                }
            }
        }
    }
}