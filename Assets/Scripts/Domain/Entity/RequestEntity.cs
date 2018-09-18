using System;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UnityEngine.SceneManagement;

namespace CAFU.Scene.Domain.Entity
{
    public interface IRequestEntity : IEntity
    {
        void RequestLoad(string sceneName);
        void RequestUnload(string sceneName);
        bool HasLoaded(string sceneName);
        IObservable<string> OnLoadRequestAsObservable();
        IObservable<string> OnUnloadRequestAsObservable();
    }

    public class RequestEntity : IRequestEntity
    {
        private ISubject<string> LoadRequestSubject { get; } = new Subject<string>();
        private ISubject<string> UnloadRequestSubject { get; } = new Subject<string>();

        public void RequestLoad(string sceneName)
        {
            LoadRequestSubject.OnNext(sceneName);
        }

        public void RequestUnload(string sceneName)
        {
            UnloadRequestSubject.OnNext(sceneName);
        }

        public bool HasLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).isLoaded;
        }

        public IObservable<string> OnLoadRequestAsObservable()
        {
            return LoadRequestSubject;
        }

        public IObservable<string> OnUnloadRequestAsObservable()
        {
            return UnloadRequestSubject;
        }
    }
}