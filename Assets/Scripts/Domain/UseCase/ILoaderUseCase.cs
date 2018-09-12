using System.Collections.Generic;
using CAFU.Core;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public interface ILoaderUseCase : IUseCase
    {
        IFactory<ISceneStrategyStructure, ISceneEntity> SceneEntityFactory { get; }

        ILoadRequestEntity LoadRequestEntity { get; }

        ISceneStateEntity SceneStateEntity { get; }

        ISceneRepository SceneRepository { get; }

        LinkedList<ISceneEntity> SceneEntityList { get; }

        void Load(ILoadRequestStructure loadRequestStructure);

        void Unload(IUnloadRequestStructure unloadRequestStructure);

        IEnumerable<ISceneStrategyStructure> GenerateInitialSceneStrategyList();
    }
}