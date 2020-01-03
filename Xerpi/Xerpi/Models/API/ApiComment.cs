using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class ApiComment
    {
        public uint Id { get; set; }
        public string Body { get; set; } // Note: Newlines come through as \r\n.
        public string Author { get; set; }

        [JsonPropertyName("image_id")]
        public uint ImageId { get; set; }

        [JsonPropertyName("posted_at")]
        public DateTimeOffset PostedAt { get; set; }

        public bool Deleted { get; set; }
    }
}
