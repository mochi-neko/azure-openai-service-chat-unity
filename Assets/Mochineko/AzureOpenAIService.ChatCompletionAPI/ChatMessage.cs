#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    [JsonObject]
    public sealed class ChatMessage
    {
        [JsonProperty("content")]
        public string? Content { get; private set; }

        [JsonProperty("role"), JsonConverter(typeof(StringEnumConverter))]
        public ChatRole? Role { get; private set; }

        public ChatMessage()
        {
            
        }
        
        public ChatMessage(
            string content,
            ChatRole role)
        {
            this.Content = content;
            this.Role = role;
        }
    }
}