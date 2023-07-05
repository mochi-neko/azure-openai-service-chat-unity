#nullable enable
using System;
using System.Net.Http.Headers;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    public sealed class BearerTokenCredential
        : IApiCredential
    {
        private string Token { get; set; }

        public BearerTokenCredential(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            
            this.Token = token;
        }

        public void Update(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            
            this.Token = token;
        }

        void IApiCredential.AddHeader(HttpRequestHeaders headers)
        {
            headers.Add("Authorization", $"Bearer {Token}");
        }
    }
}