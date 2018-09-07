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
        void RequestLoad(string sceneName, bool loadAsSingle = false, bool canLoadMultiple = false);
        void RequestLoad<TSceneName>(TSceneName sceneName, bool loadAsSingle = false, bool canLoadMultiple = false) where TSceneName : struct;
        void RequestUnload(string sceneName);
        void RequestUnload<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        bool HasLoaded(string sceneName);
        bool HasLoaded<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        IObservable<ILoadRequestStructure> OnLoadRequestAsObservable();
        IObservable<IUnloadRequestStructure> OnUnloadRequestAsObservable();
    }

    public class LoadRequestEntity : ILoadRequestEntity
    {
        private ISubject<ILoadRequestStructure> LoadRequestSubject { get; } = new Subject<ILoadRequestStructure>();
        private ISubject<IUnloadRequestStructure> UnloadRequestSubject { get; } = new Subject<IUnloadRequestStructure>();

        [Inject] private IFactory<string, bool, bool, ILoadRequestStructure> LoadRequestStructureFactory { get; }
        [Inject] private IFactory<string, IUnloadRequestStructure> UnloadRequestStructureFactory { get; }
        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)] private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        public void RequestLoad(string sceneName, bool loadAsSingle = false, bool canLoadMultiple = false)
        {
            LoadRequestSubject.OnNext(LoadRequestStructureFactory.Create(sceneName, loadAsSingle, canLoadMultiple));
        }

        public void RequestLoad<TSceneName>(TSceneName sceneName, bool loadAsSingle = false, bool canLoadMultiple = false)
            // C# 7.0 以降であれば Enum constraint が使える
            where TSceneName : struct
        {
            RequestLoad(sceneName.ToString(), loadAsSingle, canLoadMultiple);
        }

        public void RequestUnload(string sceneName)
        {
            UnloadRequestSubject.OnNext(UnloadRequestStructureFactory.Create(sceneName));
        }

        public void RequestUnload<TSceneName>(TSceneName sceneName)
            // C# 7.0 以降であれば Enum constraint が使える
            where TSceneName : struct
        {
            RequestUnload(sceneName.ToString());
        }

        public bool HasLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(SceneNameCompleter(sceneName)).isLoaded;
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
    }
}