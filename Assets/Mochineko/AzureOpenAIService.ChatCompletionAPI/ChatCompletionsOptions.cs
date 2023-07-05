#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    [JsonObject]
    public sealed class ChatCompletionsOptions
    {
        [JsonProperty("messages"), JsonRequired]
        public IList<ChatMessage> Messages { get; }

        [JsonProperty("temperature")]
        public float? Temperature { get; }
        
        [JsonProperty("top_p")]
        public float? NucleusSamplingFactor { get; }

        [JsonProperty("n")]
        public int? ChoicesPerPrompt { get; }

        [JsonProperty("stream")]
        public bool? Stream { get; }
        
        [JsonProperty("stop")]
        public IList<string>? StopSequences { get; }

        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; }

        [JsonProperty("presence_penalty")]
        public float? PresencePenalty { get; }

        [JsonProperty("frequency_penalty")]
        public float? FrequencyPenalty { get; }

        [JsonProperty("logit_bias")]
        public IDictionary<int, int>? TokenSelectionBiases { get; }

        [JsonProperty("user")]
        public string? User { get; }
        
        public ChatCompletionsOptions(
            IList<ChatMessage> messages,
            float? temperature = null,
            float? nucleusSamplingFactor = null,
            int? choicesPerPrompt = null,
            bool? stream = null,
            IList<string>? stopSequences = null,
            int? maxTokens = null,
            float? presencePenalty = null,
            float? frequencyPenalty = null,
            IDictionary<int, int>? tokenSelectionBiases = null,
            string? user = null)
        {
            this.Messages = messages;
            this.Temperature = temperature;
            this.NucleusSamplingFactor = nucleusSamplingFactor;
            this.ChoicesPerPrompt = choicesPerPrompt;
            this.Stream = stream;
            this.StopSequences = stopSequences;
            this.MaxTokens = maxTokens;
            this.PresencePenalty = presencePenalty;
            this.FrequencyPenalty = frequencyPenalty;
            this.TokenSelectionBiases = tokenSelectionBiases;
            this.User = user;
        }
    }
}