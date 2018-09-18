using System;
using CAFU.Core;

namespace CAFU.Scene.Domain.UseCase
{
    public interface IRequestHandlerPresenter : IPresenter
    {
        IObservable<string> RequestLoadSceneAsObservable();
        IObservable<string> RequestUnloadSceneAsObservable();
    }
}