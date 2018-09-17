using System.Collections.Generic;

namespace CAFU.Scene.Application
{
    public interface ISystemController
    {
        IEnumerable<string> InitialSceneNameList { get; }
    }
}