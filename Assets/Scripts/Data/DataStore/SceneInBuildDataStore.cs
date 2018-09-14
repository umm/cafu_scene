using System;
using System.Threading.Tasks;
using CAFU.Scene.Application;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Structure;
using Zenject;

namespace CAFU.Scene.Data.DataStore
{
    public class SceneInBuildDataStore : ISceneDataStore
    {
        [Inject] private IFactory<string, IScene> SceneStructureFactory { get; }

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        public async Task<IScene> GetAsync(ISceneStrategy sceneStrategy)
        {
            return await
                Task.FromResult(
                    SceneStructureFactory.Create(
                        sceneStrategy.ShouldApplyCompleter
                            ? SceneNameCompleter(sceneStrategy.SceneName)
                            : sceneStrategy.SceneName
                    )
                );
        }
    }
}