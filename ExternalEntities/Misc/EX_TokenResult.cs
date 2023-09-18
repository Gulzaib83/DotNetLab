using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExternalEntities.Misc
{
    public class EX_TokenResult
    {
        [JsonPropertyName("Token")]
        public string Token { get; set; }

        [JsonPropertyName("Status")]
        public bool Status { get; set; }

        [JsonPropertyName("RefreshToken")]
        public string RefreshToken { get; set; }
    }
}
