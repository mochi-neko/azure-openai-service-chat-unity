#nullable enable
using Newtonsoft.Json;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    [JsonObject]
    public sealed class ChatChoice
    {
        [JsonProperty("message"), JsonRequired]
        public ChatMessage Message { get; private set; } = new ();
        
        [JsonProperty("index")]
        public int? Index { get; private set; }
        
        [JsonProperty("finish_reason"), JsonRequired]
        public string FinishReason { get; private set; } = string.Empty;
    }
}