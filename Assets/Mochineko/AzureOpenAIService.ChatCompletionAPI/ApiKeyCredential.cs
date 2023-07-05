#nullable enable
using System;
using System.Net.Http.Headers;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    public sealed class ApiKeyCredential
        : IApiCredential
    {
        private string ApiKey { get; set; }

        public ApiKeyCredential(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            this.ApiKey = apiKey;
        }

        public void Update(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey));
            }

            this.ApiKey = apiKey;
        }

        void IApiCredential.AddHeader(HttpRequestHeaders headers)
        {
            headers.Add("api-key", $"{ApiKey}");
        }
    }
}