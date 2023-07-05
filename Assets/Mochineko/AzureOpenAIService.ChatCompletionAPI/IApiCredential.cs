#nullable enable
using System.Net.Http.Headers;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    public interface IApiCredential
    {
        void AddHeader(HttpRequestHeaders headers);
    }
}