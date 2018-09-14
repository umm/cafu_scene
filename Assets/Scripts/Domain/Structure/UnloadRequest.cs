using System;
using UniRx;
using Zenject;

namespace CAFU.Scene.Domain.Structure
{
    public interface IUnloadRequest
    {
        void Request(string sceneName);
        void Request<TSceneName>(TSceneName sceneName) where TSceneName : struct;
        void Request(ISceneStrategy sceneStrategy);
        void Request<TSceneName>(ISceneStrategy<TSceneName> sceneStrategy) where TSceneName : struct;
        IObservable<ISceneStrategy> OnRequestAsObservable();
        ISceneStrategy SceneStrategy { get; }
    }

    public class UnloadRequest : IUnloadRequest
    {
        private IReactiveProperty<ISceneStrategy> RequestProperty { get; } = new ReactiveProperty<ISceneStrategy>();
        [Inject] private IFactory<string, ISceneStrategy> SceneStrategyFactory { get; }

        public void Request(string sceneName)
        {
            RequestProperty.Value = SceneStrategyFactory.Create(sceneName);
        }

        public void Request<TSceneName>(TSceneName sceneName) where TSceneName : struct
        {
            RequestProperty.Value = SceneStrategyFactory.Create(sceneName.ToString());
        }

        public void Request(ISceneStrategy sceneStrategy)
        {
            RequestProperty.Value = sceneStrategy;
        }

        public void Request<TSceneName>(ISceneStrategy<TSceneName> sceneStrategy) where TSceneName : struct
        {
            RequestProperty.Value = sceneStrategy;
        }

        public IObservable<ISceneStrategy> OnRequestAsObservable()
        {
            return RequestProperty;
        }

        public ISceneStrategy SceneStrategy => RequestProperty.Value;
    }
}