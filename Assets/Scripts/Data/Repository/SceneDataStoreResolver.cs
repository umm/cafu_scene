using System.Collections.Generic;
using System.IO;
using System.Linq;
using CAFU.Core;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Structure;
using UnityEngine.SceneManagement;
using Zenject;

namespace CAFU.Scene.Data.Repository
{
    public class SceneDataStoreResolver : IResolver<ISceneStrategy, ISceneDataStore>
    {
        [Inject(Id = Constant.InjectId.DataStore.SceneInBuild)]
        private ISceneDataStore InBuildDataStore { get; set; }

        [Inject(Id = Constant.InjectId.DataStore.SceneInAssetBundle)]
        private ISceneDataStore InAssetBundleDataStore { get; set; }

        public ISceneDataStore Resolve(ISceneStrategy param1)
        {
            return
                InBuildScenePathList
                    .Any(
                        scenePath =>
                            // パスは Assets/Scenes/Foo/Bar.unity という形式なのでシーン名とのマッチは拡張子を除く
                            Path.GetFileNameWithoutExtension(scenePath) == param1.SceneName
                    )
                    ? InBuildDataStore
                    : InAssetBundleDataStore;
        }

        // Scene のパス一覧を事前に構築しておく
        //   LoadScene 後でないと Scene 構造体が構築されず Scene.name を参照出来ないため、パスの一覧から判定するしかない
        private static IEnumerable<string> InBuildScenePathList { get; } =
            Enumerable
                .Range(0, SceneManager.sceneCountInBuildSettings)
                .Select(SceneUtility.GetScenePathByBuildIndex);
    }
}
