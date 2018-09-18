using System;
using CAFU.Core;

namespace CAFU.Scene.Presentation.Presenter
{
    public interface ISceneLoadRequestable : IView
    {
        IObservable<string> RequestLoadAsObservable();
    }
}