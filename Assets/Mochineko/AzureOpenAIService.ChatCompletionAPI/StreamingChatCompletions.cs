#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    [JsonObject]
    public sealed class StreamingChatCompletions
    {
        [JsonProperty("id"), JsonRequired]
        public string Id { get; private set; } = string.Empty;
        
        [JsonProperty("object"), JsonRequired]
        public string Object { get; private set; } = string.Empty;

        [JsonProperty("created"), JsonRequired]
        public DateTime Created { get; private set; }
        
        [JsonProperty("model"), JsonRequired]
        public string Model { get; private set; } = string.Empty;

        [JsonProperty("choices"), JsonRequired]
        public IReadOnlyList<ChatChoice> Choices { get; private set; } = new List<ChatChoice>();
    }
}