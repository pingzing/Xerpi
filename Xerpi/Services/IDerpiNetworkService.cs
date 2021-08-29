using System.Collections.Generic;
using System.Threading.Tasks;
using Xerpi.Models;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public interface IDerpiNetworkService
    {
        Task<ImageSearchResponse?> GetImages(uint page = 1, uint perPage = 15);
        Task<IEnumerable<ApiTag>?> GetTags(IEnumerable<uint> ids);
        Task<ImageSearchResponse?> SearchImages(SearchParameters parameters, uint page, uint itemsPerPage);
        Task<IEnumerable<ApiFilter>?> GetDefaultFilters();

        /// <summary>
        /// Seems to return up to 30 comments.
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<CommentsResponse?> GetComments(uint imageId, uint page = 1);

        Task<ApiUser?> GetUserProfile(uint userId);
        string BaseUri { get; }
    }
}
