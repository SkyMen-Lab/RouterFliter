using Newtonsoft.Json;

namespace RouterFilter.Models
{
    public class JsonConfigs
    {
        [JsonProperty("TeamCode")]
        public string TeamCode { get; set; }

        [JsonProperty("GameCode")]
        public string GameCode { get; set; }

        [JsonProperty("IsJoining")]
        public bool IsJoining { get; set; }

    }
}
