using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExternalEntities
{
    public class Ex_ToDo
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("IsCompleted")]
        public bool? IsCompleted { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }  
    }
}
