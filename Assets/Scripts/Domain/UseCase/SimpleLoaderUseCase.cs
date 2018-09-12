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
        [Inject] IFactory<ISceneStrategyStructure, ISceneEntity> ILoaderUseCase.SceneEntityFactory { get; }

        [Inject] ILoadRequestEntity ILoaderUseCase.LoadRequestEntity { get; }

        [Inject] ISceneStateEntity ILoaderUseCase.SceneStateEntity { get; }

        [Inject] ISceneRepository ILoaderUseCase.SceneRepository { get; }

        LinkedList<ISceneEntity> ILoaderUseCase.SceneEntityList { get; } = new LinkedList<ISceneEntity>();

        [Inject] private IEnumerable<string> InitialSceneNameList { get; }

        private IDictionary<string, ISceneStrategyStructure> SceneStrategyStructureMap { get; } = new Dictionary<string, ISceneStrategyStructure>();

        void IInitializable.Initialize()
        {
            this.InitializeUseCase();
            ((ILoaderUseCase)this).LoadRequestEntity.SetSceneStrategyStructureResolver(GetOrCreateSceneStrategyStructure);
        }

        public void Load(ILoadRequestStructure loadRequestStructure)
        {
            this.LoadAsObservable(loadRequestStructure.SceneStrategyStructure).Subscribe();
        }

        public void Unload(IUnloadRequestStructure unloadRequestStructure)
        {
            this.UnloadAsObservable(unloadRequestStructure.SceneStrategyStructure).Subscribe();
        }

        public IEnumerable<ISceneStrategyStructure> GenerateInitialSceneStrategyList()
        {
            return InitialSceneNameList.Select(GetOrCreateSceneStrategyStructure);
        }

        private ISceneStrategyStructure GetOrCreateSceneStrategyStructure(string sceneName)
        {
            if (!SceneStrategyStructureMap.ContainsKey(sceneName))
            {
                SceneStrategyStructureMap[sceneName] = new SceneStrategyStructure(sceneName);
            }

            return SceneStrategyStructureMap[sceneName];
        }
    }
}