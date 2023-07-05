#nullable enable
using System;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    public sealed class ChatCompletionApiParameters
    {
        public string Path { get; }

        public ChatCompletionApiParameters(
            string yourResourceName,
            string deploymentID,
            string apiVersion)
        {
            if (string.IsNullOrWhiteSpace(yourResourceName))
            {
                throw new ArgumentNullException(nameof(yourResourceName));
            }
            if (string.IsNullOrWhiteSpace(deploymentID))
            {
                throw new ArgumentNullException(nameof(deploymentID));
            }
            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                throw new ArgumentNullException(nameof(apiVersion));
            }

            this.Path =
                $"https://{yourResourceName}.openai.azure.com/openai/deployments/{deploymentID}/chat/completions"
                + $"?api-version={apiVersion}";
        }
    }
}