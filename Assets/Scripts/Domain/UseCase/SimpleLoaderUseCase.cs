using System.Collections.Generic;
using System.Linq;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using UniRx;
using Zenject;

namespace CAFU.Scene.Domain.UseCase
{
    public class SimpleLoaderUseCase : ILoaderUseCase, IInitializable
    {
        [Inject] IFactory<ISceneStrategy, ISceneEntity> ILoaderUseCase.SceneEntityFactory { get; }

        [Inject] ILoadRequestEntity ILoaderUseCase.LoadRequestEntity { get; }

        [Inject] ISceneStateEntity ILoaderUseCase.SceneStateEntity { get; }

        [Inject] ISceneRepository ILoaderUseCase.SceneRepository { get; }

        LinkedList<ISceneEntity> ILoaderUseCase.SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        [Inject] private IEnumerable<string> InitialSceneNameList { get; }

        private IDictionary<string, ISceneStrategy> SceneStrategyStructureMap { get; } = new Dictionary<string, ISceneStrategy>();

        void IInitializable.Initialize()
        {
            this.InitializeUseCase();
            ((ILoaderUseCase)this).LoadRequestEntity.SetSceneStrategyStructureResolver(GetOrCreateSceneStrategyStructure);
        }

        public void Load(ILoadRequest loadRequest)
        {
            this.LoadAsObservable(loadRequest.SceneStrategy).Subscribe();
        }

        public void Unload(IUnloadRequest unloadRequest)
        {
            this.UnloadAsObservable(unloadRequest.SceneStrategy).Subscribe();
        }

        public IEnumerable<ISceneStrategy> GenerateInitialSceneStrategyList()
        {
            return InitialSceneNameList.Select(GetOrCreateSceneStrategyStructure);
        }

        private ISceneStrategy GetOrCreateSceneStrategyStructure(string sceneName)
        {
            if (!SceneStrategyStructureMap.ContainsKey(sceneName))
            {
                SceneStrategyStructureMap[sceneName] = new SceneStrategy(sceneName);
            }

            return SceneStrategyStructureMap[sceneName];
        }
    }
}