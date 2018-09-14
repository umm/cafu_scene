using System;
using CAFU.Core;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Domain.Entity
{
    public interface ILoadRequestEntity : IEntity
    {
        void RequestLoad(string sceneName);
        void RequestLoad<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        void RequestUnload(string sceneName);
        void RequestUnload<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        bool HasLoaded(string sceneName);
        bool HasLoaded<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        IObservable<ILoadRequest> OnLoadRequestAsObservable();
        IObservable<IUnloadRequest> OnUnloadRequestAsObservable();
        void SetSceneStrategyStructureResolver(Func<string, ISceneStrategy> resolver);
    }

    public class LoadRequestEntity : ILoadRequestEntity
    {
        private ISubject<ILoadRequest> LoadRequestSubject { get; } = new Subject<ILoadRequest>();
        private ISubject<IUnloadRequest> UnloadRequestSubject { get; } = new Subject<IUnloadRequest>();

        [Inject] private IFactory<ISceneStrategy, ILoadRequest> LoadRequestFactory { get; }
        [Inject] private IFactory<ISceneStrategy, IUnloadRequest> UnloadRequestFactory { get; }

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        private Func<string, ISceneStrategy> SceneStrategyStructureResolver { get; set; }

        public void RequestLoad(string sceneName)
        {
            var sceneStrategyStructure = SceneStrategyStructureResolver(sceneName);
            LoadRequestSubject.OnNext(LoadRequestFactory.Create(sceneStrategyStructure));
        }

        public void RequestLoad<TSceneName>(TSceneName sceneName)
            // C# 7.0 以降であれば Enum constraint が使える
            where TSceneName : struct
        {
            RequestLoad(sceneName.ToString());
        }

        public void RequestUnload(string sceneName)
        {
            var sceneStrategyStructure = SceneStrategyStructureResolver(sceneName);
            UnloadRequestSubject.OnNext(UnloadRequestFactory.Create(sceneStrategyStructure));
        }

        public void RequestUnload<TSceneName>(TSceneName sceneName)
            // C# 7.0 以降であれば Enum constraint が使える
            where TSceneName : struct
        {
            RequestUnload(sceneName.ToString());
        }

        public bool HasLoaded(string sceneName)
        {
            var sceneStrategyStructure = SceneStrategyStructureResolver(sceneName);
            return
                SceneManager
                    .GetSceneByName(
                        sceneStrategyStructure.ShouldApplyCompleter
                            ? SceneNameCompleter(sceneStrategyStructure.SceneName)
                            : sceneStrategyStructure.SceneName
                    )
                    .isLoaded;
        }

        public bool HasLoaded<TSceneName>(TSceneName sceneName)
            // C# 7.0 以降であれば Enum constraint が使える
            where TSceneName : struct
        {
            return HasLoaded(sceneName.ToString());
        }

        public IObservable<ILoadRequest> OnLoadRequestAsObservable()
        {
            return LoadRequestSubject;
        }

        public IObservable<IUnloadRequest> OnUnloadRequestAsObservable()
        {
            return UnloadRequestSubject;
        }

        public void SetSceneStrategyStructureResolver(Func<string, ISceneStrategy> resolver)
        {
            SceneStrategyStructureResolver = resolver;
        }
    }
}