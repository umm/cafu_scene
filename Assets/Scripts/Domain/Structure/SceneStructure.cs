using CAFU.Core;

namespace CAFU.Scene.Domain.Structure
{
    public interface ISceneStructure : IStructure
    {
        string Name { get; }
    }

    public struct SceneStructure : ISceneStructure
    {
        public SceneStructure(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}