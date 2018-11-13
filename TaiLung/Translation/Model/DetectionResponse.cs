using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaiLung.Translation.Model
{
    internal class DetectionResponse
    {
        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
