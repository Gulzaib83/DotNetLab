using System.Text.Json.Serialization;

namespace AWSLambdaWeb.Models
{
    [Serializable]
    public class ToDos
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("isCompleted")]
        public bool? IsCompleted { get; set; }

        [JsonPropertyName("owner")]
        public int Owner { get; set; }
    }
}
