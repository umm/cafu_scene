using CAFU.Core;

namespace CAFU.Scene.Domain.Structure
{
    public interface ILoadRequestStructure : IStructure
    {
        string SceneName { get; }
        bool CanLoadMultiple { get; }
        bool LoadAsSingle { get; }
    }

    public struct LoadRequestStructure : ILoadRequestStructure
    {
        public LoadRequestStructure(string sceneName, bool canLoadMultiple, bool loadAsSingle)
        {
            SceneName = sceneName;
            CanLoadMultiple = canLoadMultiple;
            LoadAsSingle = loadAsSingle;
        }

        public string SceneName { get; }
        public bool CanLoadMultiple { get; }
        public bool LoadAsSingle { get; }
    }

    public interface IUnloadRequestStructure : IStructure
    {
        string SceneName { get; }
    }

    public struct UnloadRequestStructure : IUnloadRequestStructure
    {
        public UnloadRequestStructure(string sceneName)
        {
            SceneName = sceneName;
        }

        public string SceneName { get; }
    }
}