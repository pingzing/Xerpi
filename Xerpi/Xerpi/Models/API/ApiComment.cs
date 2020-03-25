using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class ApiComment
    {
        public uint Id { get; set; }
        public string Body { get; set; } = null!; // Note: Newlines come through as \r\n.
        public string Author { get; set; } = null!;

        [JsonPropertyName("image_id")]
        public uint ImageId { get; set; }

        [JsonPropertyName("user_id")]
        public uint? UserId { get; set; }
    }
}
