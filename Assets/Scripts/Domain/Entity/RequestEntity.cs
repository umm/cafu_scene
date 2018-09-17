using System;
using CAFU.Core;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Domain.Entity
{
    public interface IRequestEntity : IEntity
    {
        void RequestLoad(ISceneStrategy sceneStrategy);
        void RequestUnload(ISceneStrategy sceneStrategy);
        bool HasLoaded(ISceneStrategy sceneStrategy);
        IObservable<ISceneStrategy> OnLoadRequestAsObservable();
        IObservable<ISceneStrategy> OnUnloadRequestAsObservable();
    }

    public class RequestEntity : IRequestEntity
    {
        private ISubject<ISceneStrategy> LoadRequestSubject { get; } = new Subject<ISceneStrategy>();
        private ISubject<ISceneStrategy> UnloadRequestSubject { get; } = new Subject<ISceneStrategy>();

        public void RequestLoad(ISceneStrategy sceneStrategy)
        {
            LoadRequestSubject.OnNext(sceneStrategy);
        }

        public void RequestUnload(ISceneStrategy sceneStrategy)
        {
            UnloadRequestSubject.OnNext(sceneStrategy);
        }

        public bool HasLoaded(ISceneStrategy sceneStrategy)
        {
            return SceneManager.GetSceneByName(sceneStrategy.SceneName).isLoaded;
        }

        public IObservable<ISceneStrategy> OnLoadRequestAsObservable()
        {
            return LoadRequestSubject;
        }

        public IObservable<ISceneStrategy> OnUnloadRequestAsObservable()
        {
            return UnloadRequestSubject;
        }
    }
}