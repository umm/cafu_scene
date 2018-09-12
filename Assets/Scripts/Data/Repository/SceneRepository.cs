using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using Zenject;

namespace CAFU.Scene.Data.Repository
{
    public class SceneRepository : ISceneRepository
    {
        [Inject] private IResolver<ISceneStrategyStructure, ISceneDataStore> SceneDataStoreResolver { get; }

        public async Task<ISceneStructure> GetAsync(ISceneStrategyStructure sceneStrategyStructure)
        {
            return await SceneDataStoreResolver.Resolve(sceneStrategyStructure).GetAsync(sceneStrategyStructure);
        }
    }
}