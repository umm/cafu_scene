using System;
using System.Text.RegularExpressions;
using CAFU.Core;
using UniRx;

namespace CAFU.Scene.Domain.Entity
{
    public interface ISceneStateEntity : IEntity
    {
        ISubject<string> WillLoadSubject { get; }
        ISubject<string> DidLoadSubject { get; }
        ISubject<string> WillUnloadSubject { get; }
        ISubject<string> DidUnloadSubject { get; }
        IObservable<string> WillLoadAsObservable(string sceneName = "", bool useRegex = false);
        IObservable<string> DidLoadAsObservable(string sceneName = "", bool useRegex = false);
        IObservable<string> WillUnloadAsObservable(string sceneName = "", bool useRegex = false);
        IObservable<string> DidUnloadAsObservable(string sceneName = "", bool useRegex = false);
    }

    public class SceneStateEntity : ISceneStateEntity
    {
        public ISubject<string> WillLoadSubject { get; } = new Subject<string>();
        public ISubject<string> DidLoadSubject { get; } = new Subject<string>();
        public ISubject<string> WillUnloadSubject { get; } = new Subject<string>();
        public ISubject<string> DidUnloadSubject { get; } = new Subject<string>();

        public IObservable<string> WillLoadAsObservable(string sceneName = "", bool useRegex = false)
        {
            return WillLoadSubject.Where(x => string.IsNullOrEmpty(sceneName) || !useRegex && x == sceneName || useRegex && Regex.IsMatch(x, sceneName));
        }

        public IObservable<string> DidLoadAsObservable(string sceneName = "", bool useRegex = false)
        {
            return DidLoadSubject.Where(x => string.IsNullOrEmpty(sceneName) || !useRegex && x == sceneName || useRegex && Regex.IsMatch(x, sceneName));
        }

        public IObservable<string> WillUnloadAsObservable(string sceneName = "", bool useRegex = false)
        {
            return WillUnloadSubject.Where(x => string.IsNullOrEmpty(sceneName) || !useRegex && x == sceneName || useRegex && Regex.IsMatch(x, sceneName));
        }

        public IObservable<string> DidUnloadAsObservable(string sceneName = "", bool useRegex = false)
        {
            return DidUnloadSubject.Where(x => string.IsNullOrEmpty(sceneName) || !useRegex && x == sceneName || useRegex && Regex.IsMatch(x, sceneName));
        }
    }
}