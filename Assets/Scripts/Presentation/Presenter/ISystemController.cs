using System.Collections.Generic;
using CAFU.Core;

namespace CAFU.Scene.Presentation.Presenter
{
    public interface ISystemController : IView
    {
        IEnumerable<string> InitialSceneNameList { get; }
        bool ShouldLoadInitialScenes { get; }
    }
}