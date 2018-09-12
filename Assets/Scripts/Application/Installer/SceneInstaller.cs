using CAFU.Scene.Data.DataStore;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using Zenject;

namespace CAFU.Scene.Application.Installer
{
    public class SceneInstaller : SceneInstaller<SimpleLoaderUseCase>
    {
    }

    public class SceneInstaller<TUseCase> : Installer<SceneInstaller<TUseCase>>
        where TUseCase : ILoaderUseCase
    {
        public override void InstallBindings()
        {
            // Structures
            Container.BindIFactory<ISceneStrategyStructure, ILoadRequestStructure>().To<LoadRequestStructure>();
            Container.BindIFactory<ISceneStrategyStructure, IUnloadRequestStructure>().To<UnloadRequestStructure>();
            Container.BindIFactory<string, ISceneStructure>().To<SceneStructure>();

            // Entities
            Container.BindInterfacesTo<LoadRequestEntity>().AsSingle();
            Container.BindInterfacesTo<SceneStateEntity>().AsSingle();
            Container.BindIFactory<ISceneStrategyStructure, ISceneEntity>().To<SceneEntity>();

            // UseCases
            Container.BindInterfacesTo<TUseCase>().AsSingle();

            // Repositories
            Container.BindInterfacesTo<SceneRepository>().AsSingle();
            Container.BindInterfacesTo<SceneDataStoreResolver>().AsSingle();

            // DataStores
            Container.Bind<ISceneDataStore>().WithId(Constant.InjectId.DataStore.SceneInBuild).To<SceneInBuildDataStore>().AsSingle();
            Container.Bind<ISceneDataStore>().WithId(Constant.InjectId.DataStore.SceneInAssetBundle).To<SceneInAssetBundleDataStore>().AsSingle();
        }
    }
}