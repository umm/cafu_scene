using System;
using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Application.Enumerate;
using CAFU.Scene.Domain.Structure;
using UniRx;
using UniRx.Async;
using UnityEngine.SceneManagement;

namespace CAFU.Scene.Domain.Entity
{
    public interface ISceneEntity : IEntity
    {
        ISceneStrategy SceneStrategy { get; }

        Task Load();
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

        public SceneEntity(ISceneStrategy sceneStrategy)
        {
            SceneStrategy = sceneStrategy;
        }

        public ISceneStrategy SceneStrategy { get; }

        public async Task Load()
        {
            LoadSubject.OnNext(Tense.Will);
            await SceneManager.LoadSceneAsync(
                SceneStrategy.SceneName,
                SceneStrategy.LoadAsSingle
                    ? LoadSceneMode.Single
                    : LoadSceneMode.Additive
            );
            LoadSubject.OnNext(Tense.Did);
            LoadSubject.OnCompleted();
        }

        public async Task Unload()
        {
            UnloadSubject.OnNext(Tense.Will);
            await SceneManager.UnloadSceneAsync(SceneStrategy.SceneName);
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
