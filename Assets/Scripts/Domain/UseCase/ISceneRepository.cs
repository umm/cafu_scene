using System.Threading.Tasks;
using CAFU.Core;
using CAFU.Scene.Domain.Structure;

namespace CAFU.Scene.Domain.UseCase
{
    public interface ISceneRepository : IRepository
    {
        Task<IScene> GetAsync(ISceneStrategy sceneStrategy);
    }
}