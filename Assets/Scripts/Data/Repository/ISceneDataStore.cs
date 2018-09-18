using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;

namespace CAFU.Scene.Data.Repository
{
    public interface ISceneDataStore : IDataStore
    {
        Task<IScene> GetAsync(ISceneStrategy sceneStrategy);
    }
}