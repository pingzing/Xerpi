using System;
using System.Collections.Generic;
using System.Text;

namespace Xerpi.Models.API
{
    public class CommentsResponse
    {
        public ApiComment[] Comments { get; set; }
        public uint Total { get; set; }
    }
}
