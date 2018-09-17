using System;
using CAFU.Core;

namespace CAFU.Scene.Presentation.Presenter
{
    public interface ISceneUnloadRequestable : IView
    {
        IObservable<string> RequestUnloadAsObservable();
    }
}