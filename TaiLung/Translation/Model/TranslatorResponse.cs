using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaiLung.Translation.Model
{
    internal class TranslatorResponse
    {
        [JsonProperty("translations")]
        public IEnumerable<TranslatorResult> Translations { get; set; }
    }
}
