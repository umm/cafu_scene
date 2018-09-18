using CAFU.Core;
using CAFU.Scene.Domain.Structure;

namespace CAFU.Scene.Domain.UseCase
{
    public interface ILoaderUseCase : IUseCase
    {
        void Load(ISceneStrategy sceneStrategy);

        void Unload(ISceneStrategy sceneStrategy);
    }
}