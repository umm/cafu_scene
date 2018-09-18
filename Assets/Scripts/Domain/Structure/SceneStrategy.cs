using System;
using System.Collections.Generic;
using System.Linq;
using CAFU.Core;
using UnityEngine;
using UnityModule;

namespace CAFU.Scene.Domain.Structure
{
    public interface ISceneStrategy : IStructure
    {
        string SceneName { get; }
        bool CanLoadMultiple { get; }
        bool LoadAsSingle { get; }
        IEnumerable<string> PreLoadSceneNameList { get; }
        IEnumerable<string> PostUnloadSceneNameList { get; }
    }

    public interface ISceneStrategy<out TSceneName> : ISceneStrategy
        where TSceneName : struct
    {
        new TSceneName SceneName { get; }
    }

    [Serializable]
    public class SceneStrategy : ISceneStrategy
    {
        [SerializeField] private string sceneName;
        [SerializeField] private bool canLoadMultiple;
        [SerializeField] private bool loadAsSingle;
        [SerializeField] private List<string> preLoadSceneNameList;
        [SerializeField] private List<string> postUnloadSceneNameList;
        public string SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        public IEnumerable<string> PreLoadSceneNameList => preLoadSceneNameList;

        public SceneStrategy(string sceneName, bool canLoadMultiple = false, bool loadAsSingle = false, List<string> preLoadSceneNameList = default(List<string>), List<string> postUnloadSceneNameList = default(List<string>))
        {
            this.sceneName = sceneName;
            this.canLoadMultiple = canLoadMultiple;
            this.loadAsSingle = loadAsSingle;
            this.preLoadSceneNameList = preLoadSceneNameList;
            this.postUnloadSceneNameList = postUnloadSceneNameList;
        }

        public IEnumerable<string> PostUnloadSceneNameList => postUnloadSceneNameList;
    }

    [Serializable]
    public abstract class SceneStrategy<TSceneName> : ISceneStrategy<TSceneName>
        where TSceneName : struct
    {
        [SerializeField] private TSceneName sceneName;
        [SerializeField] private bool canLoadMultiple;
        [SerializeField] private bool loadAsSingle;
        [SerializeField] private List<TSceneName> preLoadSceneNameList;
        [SerializeField] private List<TSceneName> postUnloadSceneNameList;
        string ISceneStrategy.SceneName => sceneName.ToString();
        TSceneName ISceneStrategy<TSceneName>.SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        IEnumerable<string> ISceneStrategy.PreLoadSceneNameList => preLoadSceneNameList.Select(x => x.ToString());
        IEnumerable<string> ISceneStrategy.PostUnloadSceneNameList => postUnloadSceneNameList.Select(x => x.ToString());
    }
}