using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CAFU.Core;
using CAFU.Scene.Application;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Data.Repository
{
    public class SceneDataStoreResolver : IResolver<string, ISceneDataStore>
    {
        [Inject(Id = Constant.InjectId.DataStore.SceneInBuild)]
        private ISceneDataStore InBuildDataStore { get; }

        [Inject(Id = Constant.InjectId.DataStore.SceneInAssetBundle)]
        private ISceneDataStore InAssetBundleDataStore { get; }

        public ISceneDataStore Resolve(string param1)
        {
            return InBuildScenePathList.Any(scenePath => Regex.IsMatch(scenePath, $"{param1}\\.unity$")) ? InBuildDataStore : InAssetBundleDataStore;
        }

        // Scene のパス一覧を事前に構築しておく
        //   LoadScene 後でないと Scene 構造体が構築されず、 Scene.name を参照出来ないため
        private static IEnumerable<string> InBuildScenePathList { get; } =
            Enumerable
                .Range(0, SceneManager.sceneCountInBuildSettings)
                .Select(SceneUtility.GetScenePathByBuildIndex);
    }
}