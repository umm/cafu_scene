using System;
using System.Threading.Tasks;
using CAFU.Scene.Application;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using Zenject;

namespace CAFU.Scene.Data.DataStore
{
    public class SceneInBuildDataStore : ISceneDataStore
    {
        [Inject] private IFactory<string, ISceneStructure> SceneStructureFactory { get; }

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        public async Task<ISceneStructure> GetAsync(string sceneName)
        {
            return await Task.FromResult(SceneStructureFactory.Create(SceneNameCompleter(sceneName)));
        }
    }
}