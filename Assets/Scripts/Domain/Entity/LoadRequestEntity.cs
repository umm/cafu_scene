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
        IObservable<ILoadRequestStructure> OnLoadRequestAsObservable();
        IObservable<IUnloadRequestStructure> OnUnloadRequestAsObservable();
        void SetSceneStrategyStructureResolver(Func<string, ISceneStrategyStructure> resolver);
    }

    public class LoadRequestEntity : ILoadRequestEntity
    {
        private ISubject<ILoadRequestStructure> LoadRequestSubject { get; } = new Subject<ILoadRequestStructure>();
        private ISubject<IUnloadRequestStructure> UnloadRequestSubject { get; } = new Subject<IUnloadRequestStructure>();

        [Inject] private IFactory<ISceneStrategyStructure, ILoadRequestStructure> LoadRequestStructureFactory { get; }
        [Inject] private IFactory<ISceneStrategyStructure, IUnloadRequestStructure> UnloadRequestStructureFactory { get; }

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        private Func<string, ISceneStrategyStructure> SceneStrategyStructureResolver { get; set; }

        public void RequestLoad(string sceneName)
        {
            var sceneStrategyStructure = SceneStrategyStructureResolver(sceneName);
            LoadRequestSubject.OnNext(LoadRequestStructureFactory.Create(sceneStrategyStructure));
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
            UnloadRequestSubject.OnNext(UnloadRequestStructureFactory.Create(sceneStrategyStructure));
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

        public IObservable<ILoadRequestStructure> OnLoadRequestAsObservable()
        {
            return LoadRequestSubject;
        }

        public IObservable<IUnloadRequestStructure> OnUnloadRequestAsObservable()
        {
            return UnloadRequestSubject;
        }

        public void SetSceneStrategyStructureResolver(Func<string, ISceneStrategyStructure> resolver)
        {
            SceneStrategyStructureResolver = resolver;
        }
    }
}