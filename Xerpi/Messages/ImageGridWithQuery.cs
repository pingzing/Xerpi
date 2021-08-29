using System;
using System.Collections.Generic;
using System.Text;
using Xerpi.Models;

namespace Xerpi.Messages
{
    /// <summary>
    /// A message or nav arg that indicates that the caller wants to navigate to the ImageGrid page, and search for a specific query.
    /// </summary>
    public class ImageGridWithQuery
    {
        public string Query { get; set; } = null!;
        public string? SortProperty { get; set; }
        public SortOrderKind? SortOrder { get; set; }
    }
}
