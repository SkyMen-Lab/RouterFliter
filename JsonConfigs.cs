using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TheP0ngServer
{
    public class JsonConfigs
    {
        [JsonProperty("SchoolCode")]
        public string SchoolCode { get; set; }

        [JsonProperty("GameCode")]
        public string GameCode { get; set; }
    }
}
