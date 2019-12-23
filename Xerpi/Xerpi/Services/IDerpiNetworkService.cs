using System.Collections.Generic;
using System.Threading.Tasks;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public interface IDerpiNetworkService
    {
        Task<IEnumerable<ApiImage>?> GetImages(uint page = 1, uint perPage = 15);
        Task<IEnumerable<ApiTag>?> GetTags(IEnumerable<uint> ids);
        Task<SearchResponse?> SearchImages(string query, uint page, uint itemsPerPage);
        Task<IEnumerable<ApiFilter>?> GetDefaultFilters();
    }
}
