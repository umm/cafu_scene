using CAFU.Core;

namespace CAFU.Scene.Domain.Structure
{
    public interface ILoadRequestStructure : IStructure
    {
        ISceneStrategyStructure SceneStrategyStructure { get; }
    }

    public struct LoadRequestStructure : ILoadRequestStructure
    {
        public LoadRequestStructure(ISceneStrategyStructure sceneStrategyStructure)
        {
            SceneStrategyStructure = sceneStrategyStructure;
        }

        public ISceneStrategyStructure SceneStrategyStructure { get; }
    }

    public interface IUnloadRequestStructure : IStructure
    {
        ISceneStrategyStructure SceneStrategyStructure { get; }
    }

    public struct UnloadRequestStructure : IUnloadRequestStructure
    {
        public UnloadRequestStructure(ISceneStrategyStructure sceneStrategyStructure)
        {
            SceneStrategyStructure = sceneStrategyStructure;
        }

        public ISceneStrategyStructure SceneStrategyStructure { get; }
    }
}