using System.Collections.Generic;
using CAFU.Core;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public interface ILoaderUseCase : IUseCase
    {
        IFactory<ISceneStrategy, ISceneEntity> SceneEntityFactory { get; }

        ILoadRequestEntity LoadRequestEntity { get; }

        ISceneStateEntity SceneStateEntity { get; }

        ISceneRepository SceneRepository { get; }

        LinkedList<ISceneEntity> SceneEntityList { get; }

        void Load(ILoadRequest loadRequest);

        void Unload(IUnloadRequest unloadRequest);

        IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList();
    }
}