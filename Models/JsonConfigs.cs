using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TheP0ngServer
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
