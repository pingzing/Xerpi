using System.Collections.Generic;
using Xerpi.Models.API;

namespace Xerpi.Models
{
    public class ApiTagComparer : IComparer<ApiTag>
    {
        public ApiTagComparer()
        {

        }

        public int Compare(ApiTag x, ApiTag y)
        {
            // Weight categories higher, as they take precedence in how tags are sorted
            int compareVal = x.Category.CompareTo(y.Category) * 2;
            compareVal += x.Name.CompareTo(y.Name);
            return compareVal;
        }
    }
}
