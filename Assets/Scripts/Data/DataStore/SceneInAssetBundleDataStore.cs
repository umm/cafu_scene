using System;
using System.Threading.Tasks;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Data.DataStore
{
    public class SceneInAssetBundleDataStore : ISceneDataStore
    {
        [Inject] private IFactory<string, IScene> SceneStructureFactory { get; set; }

        public Task<IScene> GetAsync(ISceneStrategy sceneStrategy)
        {
            throw new NotImplementedException();
        }
    }
}