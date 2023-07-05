#nullable enable
using Newtonsoft.Json;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    [JsonObject]
    public sealed class CompletionsUsage
    {
        [JsonProperty("prompt_tokens"), JsonRequired]
        public int PromptTokens { get; private set; }

        [JsonProperty("completion_tokens"), JsonRequired]
        public int CompletionTokens { get; private set; }

        [JsonProperty("total_tokens"), JsonRequired]
        public int TotalTokens { get; private set; }
    }
}