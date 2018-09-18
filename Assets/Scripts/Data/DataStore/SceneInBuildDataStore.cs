using System.Threading.Tasks;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Data.DataStore
{
    public class SceneInBuildDataStore : ISceneDataStore
    {
        [Inject] private IFactory<string, IScene> SceneStructureFactory { get; }

        public async Task<IScene> GetAsync(ISceneStrategy sceneStrategy)
        {
            return await Task.FromResult(SceneStructureFactory.Create(sceneStrategy.SceneName));
        }
    }
}