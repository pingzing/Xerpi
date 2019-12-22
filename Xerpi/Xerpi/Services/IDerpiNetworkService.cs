using System.Threading.Tasks;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public interface IDerpiNetworkService
    {
        Task<ImagesResponse?> GetImages(uint page = 1, uint perPage = 15);
        Task<TagsResponse?> GetTags(uint[] ids);
    }
}
