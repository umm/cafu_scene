using System;
using System.Linq;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UnityEngine.SceneManagement;

namespace CAFU.Scene.Domain.UseCase
{
    public static class LoaderUseCaseExtensions
    {
        public static void InitializeUseCase(this ILoaderUseCase loaderUseCase)
        {
            loaderUseCase.LoadRequestEntity.OnLoadRequestAsObservable().Subscribe(loaderUseCase.Load);
            loaderUseCase.LoadRequestEntity.OnUnloadRequestAsObservable().Subscribe(loaderUseCase.Unload);
            SceneManager.sceneLoaded += (scene, loadSceneMode) => loaderUseCase.SceneStateEntity.DidLoadSubject.OnNext(scene.name);
            SceneManager.sceneUnloaded += (scene) => loaderUseCase.SceneStateEntity.DidUnloadSubject.OnNext(scene.name);
            loaderUseCase.GenerateInitialSceneStrategyList().Select(loaderUseCase.SceneEntityFactory.Create).ToList().ForEach(x => loaderUseCase.SceneEntityList.AddLast(x));
        }

        public static IObservable<Unit> LoadAsObservable(this ILoaderUseCase loaderUseCase, ISceneStrategyStructure sceneStrategyStructure)
        {
            // Do nothing if duplicate loading
            if (!sceneStrategyStructure.CanLoadMultiple && loaderUseCase.LoadRequestEntity.HasLoaded(sceneStrategyStructure.SceneName))
            {
                return Observable.ReturnUnit();
            }
            var sceneEntity = loaderUseCase.SceneEntityFactory.Create(sceneStrategyStructure);
            loaderUseCase.SceneEntityList.AddLast(sceneEntity);
            loaderUseCase.SceneStateEntity.WillLoadSubject.OnNext(sceneEntity.SceneStrategyStructure.SceneName);
            return loaderUseCase.SceneRepository
                .GetAsync(sceneStrategyStructure)
                .ToObservable()
                .SelectMany(
                    _ => sceneEntity
                        .Load()
                        .ToObservable()
                );
        }

        public static IObservable<Unit> UnloadAsObservable(this ILoaderUseCase loaderUseCase, ISceneStrategyStructure sceneStrategyStructure)
        {
            if (loaderUseCase.SceneEntityList.All(x => x.SceneStrategyStructure.SceneName != sceneStrategyStructure.SceneName))
            {
                return Observable.Throw<Unit>(new ArgumentOutOfRangeException($"Scene name `{sceneStrategyStructure.SceneName}' does not contain in loaded scenes."));
            }

            loaderUseCase.SceneStateEntity.WillUnloadSubject.OnNext(sceneStrategyStructure.SceneName);
            // Search from last
            var sceneEntity = loaderUseCase.SceneEntityList.Last(x => x.SceneStrategyStructure.SceneName == sceneStrategyStructure.SceneName);
            return sceneEntity
                .Unload()
                .ToObservable()
                .ForEachAsync(_ => loaderUseCase.SceneEntityList.Remove(sceneEntity));
        }
    }
}