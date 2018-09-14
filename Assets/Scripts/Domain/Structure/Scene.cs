using CAFU.Core;

namespace CAFU.Scene.Domain.Structure
{
    public interface IScene : IStructure
    {
        string Name { get; }
    }

    public struct Scene : IScene
    {
        public Scene(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}