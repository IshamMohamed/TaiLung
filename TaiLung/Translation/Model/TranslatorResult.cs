using Newtonsoft.Json;

namespace TaiLung.Translation.Model
{
    internal class TranslatorResult
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }
}
