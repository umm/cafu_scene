using System.Collections.Generic;
using CAFU.Core;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public interface ILoaderUseCase : IUseCase
    {
        IFactory<string, ISceneEntity> SceneEntityFactory { get; }

        ILoadRequestEntity LoadRequestEntity { get; }

        ISceneStateEntity SceneStateEntity { get; }

        ISceneRepository SceneRepository { get; }

        LinkedList<ISceneEntity> SceneEntityList { get; }

        IEnumerable<string> InitialSceneNameList { get; }

        void Load(ILoadRequestStructure loadRequestStructure);

        void Unload(IUnloadRequestStructure unloadRequestStructure);
    }
}