using System.Collections.Generic;
using Xerpi.Models.API;

namespace Xerpi.Models
{
    public class SearchPageComparer : IComparer<ApiImage>
    {
        public int Compare(ApiImage x, ApiImage y)
        {
            if (x.SearchPage!.Value == y.SearchPage!.Value)
            {
                return x.SortIndex!.Value.CompareTo(y.SortIndex!.Value);
            }
            else
            {
                return x.SearchPage.Value.CompareTo(y.SearchPage.Value);
            }
        }
    }
}
