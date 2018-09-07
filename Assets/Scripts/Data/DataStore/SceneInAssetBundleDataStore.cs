using System;
using System.Threading.Tasks;
using CAFU.Scene.Application;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Data.DataStore
{
    public class SceneInAssetBundleDataStore : ISceneDataStore
    {
        [Inject] private IFactory<string, ISceneStructure> SceneStructureFactory { get; }

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        public Task<ISceneStructure> GetAsync(string sceneName)
        {
            throw new NotImplementedException();
        }
    }
}