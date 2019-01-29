using System;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using CAFU.Zenject.Utility;
using UniRx;
using Zenject;

namespace CAFU.Scene.Presentation.Presenter
{
    public class RequestHandlerPresenter :
        IRequestHandlerPresenter,
        IInstanceReceiver,
        IInitializable
    {
        [Inject] IMessageReceiver IInstanceReceiver.MessageReceiver { get; set; }
        [Inject] private IFactory<string, ISceneStrategy> SceneStrategyFactory { get; set; }

        private ISubject<string> RequestLoadSubject { get; } = new Subject<string>();
        private ISubject<string> RequestUnloadSubject { get; } = new Subject<string>();

        void IInitializable.Initialize()
        {
            // Subscribe IObservable per instantiate ISceneLoadRequestable/ISceneUnloadRequestable
            this.Receive<ISceneLoadRequestable>()
                .SelectMany(x => x.RequestLoadAsObservable())
                .Subscribe(RequestLoadSubject);
            this.Receive<ISceneUnloadRequestable>()
                .SelectMany(x => x.RequestUnloadAsObservable())
                .Subscribe(RequestUnloadSubject);
        }

        public IObservable<string> RequestLoadSceneAsObservable()
        {
            return RequestLoadSubject;
        }

        public IObservable<string> RequestUnloadSceneAsObservable()
        {
            return RequestUnloadSubject;
        }
    }
}