using CAFU.Scene.Data.DataStore;
using CAFU.Scene.Data.Repository;
using CAFU.Scene.Domain.Entity;
using CAFU.Scene.Domain.Structure;
using CAFU.Scene.Domain.UseCase;
using CAFU.Scene.Presentation.Presenter;
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
            Container.BindIFactory<string, ISceneStrategy>().To<SceneStrategy>();
            Container.BindIFactory<string, IScene>().To<Domain.Structure.Scene>();

            // Entities
            Container.BindInterfacesTo<RequestEntity>().AsSingle();
            Container.BindInterfacesTo<SceneStateEntity>().AsSingle();
            Container.BindIFactory<ISceneStrategy, ISceneEntity>().To<SceneEntity>();

            // UseCases
            Container.BindInterfacesTo<TUseCase>().AsSingle();

            // Repositories
            Container.BindInterfacesTo<SceneRepository>().AsSingle();
            Container.BindInterfacesTo<SceneDataStoreResolver>().AsSingle();

            // DataStores
            Container.Bind<ISceneDataStore>().WithId(Constant.InjectId.DataStore.SceneInBuild).To<SceneInBuildDataStore>().AsSingle();
            Container.Bind<ISceneDataStore>().WithId(Constant.InjectId.DataStore.SceneInAssetBundle).To<SceneInAssetBundleDataStore>().AsSingle();

            // Presenters
            Container.BindInterfacesTo<RequestHandlerPresenter>().AsSingle();
        }
    }
}