using System.Collections.Generic;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using UniRx;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public class SimpleLoaderUseCase : ILoaderUseCase, IInitializable
    {
        [Inject] IFactory<string, ISceneEntity> ILoaderUseCase.SceneEntityFactory { get; }

        [Inject] ILoadRequestEntity ILoaderUseCase.LoadRequestEntity { get; }

        [Inject] ISceneStateEntity ILoaderUseCase.SceneStateEntity { get; }

        [Inject] ISceneRepository ILoaderUseCase.SceneRepository { get; }

        LinkedList<ISceneEntity> ILoaderUseCase.SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        [Inject] IEnumerable<string> ILoaderUseCase.InitialSceneNameList { get; }

        void IInitializable.Initialize()
        {
            this.InitializeUseCase();
        }

        public void Load(ILoadRequestStructure loadRequestStructure)
        {
            this.LoadAsObservable(loadRequestStructure.SceneName, loadRequestStructure.LoadAsSingle, loadRequestStructure.CanLoadMultiple).Subscribe();
        }

        public void Unload(IUnloadRequestStructure unloadRequestStructure)
        {
            this.UnloadAsObservable(unloadRequestStructure.SceneName).Subscribe();
        }
    }
}