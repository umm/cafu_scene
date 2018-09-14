using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using Zenject;

namespace CAFU.Scene.Data.Repository
{
    public class SceneRepository : ISceneRepository
    {
        [Inject] private IResolver<ISceneStrategy, ISceneDataStore> SceneDataStoreResolver { get; }

        public async Task<IScene> GetAsync(ISceneStrategy sceneStrategy)
        {
            return await SceneDataStoreResolver.Resolve(sceneStrategy).GetAsync(sceneStrategy);
        }
    }
}