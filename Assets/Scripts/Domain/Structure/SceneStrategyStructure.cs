using System;
using System.Collections.Generic;
using System.Linq;
using CAFU.Core;
using UnityEngine;

namespace CAFU.Scene.Domain.Structure
{
    public interface ISceneStrategyStructure : IStructure
    {
        string SceneName { get; }
        bool CanLoadMultiple { get; }
        bool LoadAsSingle { get; }
        bool ShouldApplyCompleter { get; }
        IEnumerable<string> PreLoadSceneNameList { get; }
        IEnumerable<string> PostUnloadSceneNameList { get; }
    }

    public interface ISceneStrategyStructure<out TSceneName> : ISceneStrategyStructure
        where TSceneName : struct
    {
        new TSceneName SceneName { get; }
        new IEnumerable<TSceneName> PreLoadSceneNameList { get; }
        new IEnumerable<TSceneName> PostUnloadSceneNameList { get; }
    }

    [Serializable]
    public class SceneStrategyStructure : ISceneStrategyStructure
    {
        [SerializeField] private string sceneName;
        [SerializeField] private bool canLoadMultiple;
        [SerializeField] private bool loadAsSingle;
        [SerializeField] private bool shouldApplyCompleter;
        [SerializeField] private List<string> preLoadSceneNameList;
        [SerializeField] private List<string> postUnloadSceneNameList;
        public string SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        public bool ShouldApplyCompleter => shouldApplyCompleter;
        public IEnumerable<string> PreLoadSceneNameList => preLoadSceneNameList;

        public SceneStrategyStructure(string sceneName, bool canLoadMultiple = false, bool loadAsSingle = false, bool shouldApplyCompleter = false, List<string> preLoadSceneNameList = default(List<string>), List<string> postUnloadSceneNameList = default(List<string>))
        {
            this.sceneName = sceneName;
            this.canLoadMultiple = canLoadMultiple;
            this.loadAsSingle = loadAsSingle;
            this.shouldApplyCompleter = shouldApplyCompleter;
            this.preLoadSceneNameList = preLoadSceneNameList;
            this.postUnloadSceneNameList = postUnloadSceneNameList;
        }

        public IEnumerable<string> PostUnloadSceneNameList => postUnloadSceneNameList;
    }

    [Serializable]
    public abstract class SceneStrategyStructure<TSceneName> : ISceneStrategyStructure<TSceneName>
        where TSceneName : struct
    {
        [SerializeField] private TSceneName sceneName;
        [SerializeField] private bool canLoadMultiple;
        [SerializeField] private bool loadAsSingle;
        [SerializeField] private bool shouldApplyCompleter;
        [SerializeField] private List<TSceneName> preLoadSceneNameList;
        [SerializeField] private List<TSceneName> postUnloadSceneNameList;
        string ISceneStrategyStructure.SceneName => sceneName.ToString();
        TSceneName ISceneStrategyStructure<TSceneName>.SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        public bool ShouldApplyCompleter => shouldApplyCompleter;
        IEnumerable<string> ISceneStrategyStructure.PreLoadSceneNameList => preLoadSceneNameList.Select(x => x.ToString());
        IEnumerable<TSceneName> ISceneStrategyStructure<TSceneName>.PreLoadSceneNameList => preLoadSceneNameList;
        IEnumerable<string> ISceneStrategyStructure.PostUnloadSceneNameList => postUnloadSceneNameList.Select(x => x.ToString());
        IEnumerable<TSceneName> ISceneStrategyStructure<TSceneName>.PostUnloadSceneNameList => postUnloadSceneNameList;
    }
}