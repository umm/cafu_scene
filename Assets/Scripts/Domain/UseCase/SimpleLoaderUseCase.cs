using System.Collections.Generic;
using System.Linq;
using CAFU.Scene.Domain.Structure;
using UniRx;

namespace CAFU.Scene.Domain.UseCase
{
    public class SimpleLoaderUseCase : LoaderUseCaseBase
    {
        public override void Load(ISceneStrategy sceneStrategy)
        {
            LoadAsObservable(sceneStrategy).Subscribe();
        }

        public override void Unload(ISceneStrategy sceneStrategy)
        {
            UnloadAsObservable(sceneStrategy).Subscribe();
        }

        protected override IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList()
        {
            return InitialSceneNameList.Select(GetOrCreateSceneStrategy);
        }

        protected override ISceneStrategy GetOrCreateSceneStrategy(string sceneName)
        {
            if (!SceneStrategyMap.ContainsKey(sceneName))
            {
                SceneStrategyMap[sceneName] = new SceneStrategy(sceneName);
            }

            return SceneStrategyMap[sceneName];
        }
    }
}