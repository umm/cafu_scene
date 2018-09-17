using System.Collections.Generic;
using System.Linq;
using CAFU.Core;
using UnityEngine;
using UnityModule;

namespace CAFU.Scene.Domain.Structure
{
    public interface ISceneStrategyList : IStructure
    {
        IDictionary<string, ISceneStrategy> AsDictionary();
    }

    public abstract class SceneStrategyListBase<TSceneStrategy> : ScriptableObject, ISceneStrategyList
        where TSceneStrategy : ISceneStrategy
    {
        [SerializeField] [ElementTitle("sceneName")]
        private List<TSceneStrategy> list;

        private IEnumerable<TSceneStrategy> List => list;

        public IDictionary<string, ISceneStrategy> AsDictionary()
        {
            return List
                .ToDictionary(
                    x => x.SceneName.ToString(),
                    x => (ISceneStrategy) x
                );
        }
    }

    public abstract class PlaceholderSceneStrategyList<TSceneName, TSceneStrategy> : SceneStrategyListBase<TSceneStrategy>
        where TSceneName : struct
        where TSceneStrategy : ISceneStrategy<TSceneName>
    {
    }
}