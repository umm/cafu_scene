using System;
using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Application;
using CAFU.Scene.Application.Enumerate;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Domain.Entity
{
    public interface ISceneEntity : IEntity
    {
        string SceneName { get; }

        Task Load(bool loadAsSingle = false);
        Task Unload();

        IObservable<Unit> WillLoadAsObservable();
        IObservable<Unit> DidLoadAsObservable();
        IObservable<Unit> WillUnloadAsObservable();
        IObservable<Unit> DidUnloadAsObservable();
    }

    public class SceneEntity : ISceneEntity
    {
        private ISubject<Tense> LoadSubject { get; } = new Subject<Tense>();
        private ISubject<Tense> UnloadSubject { get; } = new Subject<Tense>();

        [InjectOptional(Id = Constant.InjectId.SceneNameCompleter)]
        private Func<string, string> SceneNameCompleter { get; } = sceneName => sceneName;

        public SceneEntity(string sceneName)
        {
            SceneName = sceneName;
        }

        public string SceneName { get; }

        public async Task Load(bool loadAsSingle = false)
        {
            LoadSubject.OnNext(Tense.Will);
            await SceneManager.LoadSceneAsync(SceneNameCompleter(SceneName), loadAsSingle ? LoadSceneMode.Single : LoadSceneMode.Additive);
            LoadSubject.OnNext(Tense.Did);
            LoadSubject.OnCompleted();
        }

        public async Task Unload()
        {
            UnloadSubject.OnNext(Tense.Will);
            await SceneManager.UnloadSceneAsync(SceneNameCompleter(SceneName));
            UnloadSubject.OnNext(Tense.Did);
            UnloadSubject.OnCompleted();
        }

        public IObservable<Unit> WillLoadAsObservable()
        {
            return LoadSubject.Where(x => x == Tense.Will).AsUnitObservable().Take(1);
        }

        public IObservable<Unit> DidLoadAsObservable()
        {
            return LoadSubject.Where(x => x == Tense.Did).AsUnitObservable().Take(1);
        }

        public IObservable<Unit> WillUnloadAsObservable()
        {
            return UnloadSubject.Where(x => x == Tense.Will).AsUnitObservable().Take(1);
        }

        public IObservable<Unit> DidUnloadAsObservable()
        {
            return UnloadSubject.Where(x => x == Tense.Did).AsUnitObservable().Take(1);
        }
    }
}