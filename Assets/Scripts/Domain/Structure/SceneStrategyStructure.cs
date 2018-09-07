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
        [SerializeField] private List<string> preLoadSceneNameList;
        [SerializeField] private List<string> postUnloadSceneNameList;
        public string SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        public IEnumerable<string> PreLoadSceneNameList => preLoadSceneNameList;
        public IEnumerable<string> PostUnloadSceneNameList => postUnloadSceneNameList;
    }

    [Serializable]
    public abstract class SceneStrategyStructure<TSceneName> : ISceneStrategyStructure<TSceneName>
        where TSceneName : struct
    {
        [SerializeField] private TSceneName sceneName;
        [SerializeField] private bool canLoadMultiple;
        [SerializeField] private bool loadAsSingle;
        [SerializeField] private List<TSceneName> preLoadSceneNameList;
        [SerializeField] private List<TSceneName> postUnloadSceneNameList;
        string ISceneStrategyStructure.SceneName => sceneName.ToString();
        TSceneName ISceneStrategyStructure<TSceneName>.SceneName => sceneName;
        public bool CanLoadMultiple => canLoadMultiple;
        public bool LoadAsSingle => loadAsSingle;
        IEnumerable<string> ISceneStrategyStructure.PreLoadSceneNameList => preLoadSceneNameList.Select(x => x.ToString());
        IEnumerable<TSceneName> ISceneStrategyStructure<TSceneName>.PreLoadSceneNameList => preLoadSceneNameList;
        IEnumerable<string> ISceneStrategyStructure.PostUnloadSceneNameList => postUnloadSceneNameList.Select(x => x.ToString());
        IEnumerable<TSceneName> ISceneStrategyStructure<TSceneName>.PostUnloadSceneNameList => postUnloadSceneNameList;
    }
}