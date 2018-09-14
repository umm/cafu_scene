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

        public static IObservable<Unit> LoadAsObservable(this ILoaderUseCase loaderUseCase, ISceneStrategy sceneStrategy)
        {
            // Do nothing if duplicate loading
            if (!sceneStrategy.CanLoadMultiple && loaderUseCase.LoadRequestEntity.HasLoaded(sceneStrategy.SceneName))
            {
                return Observable.ReturnUnit();
            }
            var sceneEntity = loaderUseCase.SceneEntityFactory.Create(sceneStrategy);
            loaderUseCase.SceneEntityList.AddLast(sceneEntity);
            loaderUseCase.SceneStateEntity.WillLoadSubject.OnNext(sceneEntity.SceneStrategy.SceneName);
            return loaderUseCase.SceneRepository
                .GetAsync(sceneStrategy)
                .ToObservable()
                .SelectMany(
                    _ => sceneEntity
                        .Load()
                        .ToObservable()
                );
        }

        public static IObservable<Unit> UnloadAsObservable(this ILoaderUseCase loaderUseCase, ISceneStrategy sceneStrategy)
        {
            if (loaderUseCase.SceneEntityList.All(x => x.SceneStrategy.SceneName != sceneStrategy.SceneName))
            {
                return Observable.Throw<Unit>(new ArgumentOutOfRangeException($"Scene name `{sceneStrategy.SceneName}' does not contain in loaded scenes."));
            }

            loaderUseCase.SceneStateEntity.WillUnloadSubject.OnNext(sceneStrategy.SceneName);
            // Search from last
            var sceneEntity = loaderUseCase.SceneEntityList.Last(x => x.SceneStrategy.SceneName == sceneStrategy.SceneName);
            return sceneEntity
                .Unload()
                .ToObservable()
                .ForEachAsync(_ => loaderUseCase.SceneEntityList.Remove(sceneEntity));
        }
    }
}