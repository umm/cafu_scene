using System;
using System.Linq;
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
            loaderUseCase.InitialSceneNameList.Select(loaderUseCase.SceneEntityFactory.Create).ToList().ForEach(x => loaderUseCase.SceneEntityList.AddLast(x));
        }

        public static IObservable<Unit> LoadAsObservable(this ILoaderUseCase loaderUseCase, string sceneName, bool loadAsSingle, bool canLoadMultiple)
        {
            // Do nothing if duplicate loading
            if (!canLoadMultiple && loaderUseCase.LoadRequestEntity.HasLoaded(sceneName))
            {
                return Observable.ReturnUnit();
            }
            var sceneEntity = loaderUseCase.SceneEntityFactory.Create(sceneName);
            loaderUseCase.SceneEntityList.AddLast(sceneEntity);
            loaderUseCase.SceneStateEntity.WillLoadSubject.OnNext(sceneEntity.SceneName);
            return loaderUseCase.SceneRepository
                .GetAsync(sceneName)
                .ToObservable()
                .SelectMany(
                    _ => sceneEntity
                        .Load(loadAsSingle)
                        .ToObservable()
                );
        }

        public static IObservable<Unit> UnloadAsObservable(this ILoaderUseCase loaderUseCase, string sceneName)
        {
            if (loaderUseCase.SceneEntityList.All(x => x.SceneName != sceneName))
            {
                return Observable.Throw<Unit>(new ArgumentOutOfRangeException($"Scene name `{sceneName}' does not contain in loaded scenes."));
            }

            loaderUseCase.SceneStateEntity.WillUnloadSubject.OnNext(sceneName);
            // Search from last
            var sceneEntity = loaderUseCase.SceneEntityList.Last(x => x.SceneName == sceneName);
            return sceneEntity
                .Unload()
                .ToObservable()
                .ForEachAsync(_ => loaderUseCase.SceneEntityList.Remove(sceneEntity));
        }
    }
}