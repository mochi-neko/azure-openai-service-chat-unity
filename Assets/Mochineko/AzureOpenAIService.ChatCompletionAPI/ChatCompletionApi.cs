#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Newtonsoft.Json;
using Unity.Logging;

namespace Mochineko.AzureOpenAIService.ChatCompletionAPI
{
    public static class ChatCompletionApi
    {
        public static async UniTask<IUncertainResult<ChatCompletions>> CompleteChatAsync(
            HttpClient httpClient,
            IApiCredential credential,
            ChatCompletionApiParameters parameters,
            ChatCompletionsOptions options,
            CancellationToken cancellationToken)
        {
            // Validations
            if (cancellationToken.IsCancellationRequested)
            {
                Log.Error("[AzureOpenAIService.ChatCompletion] Already cancelled.");
                return UncertainResults.RetryWithTrace<ChatCompletions>("Already cancelled.");
            }
            
            Log.Debug("[AzureOpenAIService.ChatCompletion] Begin to complete chat.");

            // Create request message
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, parameters.Path);
            
            // Add headers
            credential.AddHeader(requestMessage.Headers);

            // Serialize options to JSON
            var serializationResult = RelentJsonSerializer.Serialize(
                options,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            switch (serializationResult)
            {
                case ISuccessResult<string> serializationSuccess:
                    var optionsJson = serializationSuccess.Result;
                    Log.Verbose("[AzureOpenAIService.ChatCompletion] Options JSON:\n{0}", optionsJson);
                    requestMessage.Content = new StringContent(
                        content: optionsJson,
                        encoding: System.Text.Encoding.UTF8,
                        mediaType: "application/json");
                    break;

                case IFailureResult<string> serializationFailure:
                    Log.Error("[AzureOpenAIService.ChatCompletion] Failed to serialize options because -> {0}.", serializationFailure.Message);
                    return UncertainResults.FailWithTrace<ChatCompletions>(
                        $"Failed to serialize options because -> {serializationFailure.Message}.");
                default:
                    Log.Error("[AzureOpenAIService.ChatCompletion] Unexpected serialization result -> {0}.", serializationResult);
                    throw new ResultPatternMatchException(nameof(serializationResult));
            }

            // Run request on a thread pool
            await UniTask.SwitchToThreadPool();

            // Send request
            HttpResponseMessage responseMessage;
            var apiResult = await UncertainTryFactory
                .TryAsync<HttpResponseMessage>(async innerCancellationToken
                    => await httpClient.SendAsync(requestMessage, innerCancellationToken))
                .CatchAsRetryable<HttpResponseMessage, HttpRequestException>(exception
                    => $"Retryable due to request exception -> {exception}.")
                .CatchAsRetryable<HttpResponseMessage, OperationCanceledException>(exception
                    => $"Retryable due to cancellation exception -> {exception}.")
                .CatchAsFailure<HttpResponseMessage, Exception>(exception
                    => $"Failure due to unhandled -> {exception}.")
                .Finalize(() =>
                {
                    requestMessage.Dispose();
                    return UniTask.CompletedTask;
                })
                .ExecuteAsync(cancellationToken);

            // Return to the main thread
            await UniTask.SwitchToMainThread();

            switch (apiResult)
            {
                case IUncertainSuccessResult<HttpResponseMessage> apiSuccess:
                    Log.Verbose("[AzureOpenAIService.ChatCompletion] Success to send request.");
                    responseMessage = apiSuccess.Result;
                    break;

                case IUncertainRetryableResult<HttpResponseMessage> apiRetryable:
                    Log.Error("[AzureOpenAIService.ChatCompletion] Retryable to send request due to {0}",
                        apiRetryable.Message);
                    return UncertainResults.RetryWithTrace<ChatCompletions>(apiRetryable.Message);

                case IUncertainFailureResult<HttpResponseMessage> apiFailure:
                    Log.Error("[AzureOpenAIService.ChatCompletion] Failure to send request due to {0}",
                        apiFailure.Message);
                    return UncertainResults.FailWithTrace<ChatCompletions>(apiFailure.Message);

                default:
                    Log.Fatal("[AzureOpenAIService.ChatCompletion] Unexpected API request result -> {0}", apiResult);
                    throw new UncertainResultPatternMatchException(nameof(apiResult));
            }

            // Dispose response message when out of scope
            using var _ = responseMessage;

            // Read response body
            if (responseMessage.Content == null)
            {
                Log.Error("[AzureOpenAIService.ChatCompletion] Response content is null.");
                return UncertainResults.FailWithTrace<ChatCompletions>("Response content is null.");
            }

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                Log.Error("[AzureOpenAIService.ChatCompletion] Response body is empty.");
                return UncertainResults.FailWithTrace<ChatCompletions>("Response body is empty.");
            }

            Log.Verbose(
                "[AzureOpenAIService.ChatCompletion] Status code:({0}){1}, response body:\n{2}",
                (int)responseMessage.StatusCode,
                responseMessage.StatusCode,
                responseText);
            
            // Success
            if (responseMessage.IsSuccessStatusCode)
            {
                Log.Verbose("[AzureOpenAIService.ChatCompletion] Succeeded to complete chat:\n{0}.", responseText);
                
                // Deserialize result from JSON
                var deserializationResult = RelentJsonSerializer.Deserialize<ChatCompletions>(
                    responseText,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                switch (deserializationResult)
                {
                    case ISuccessResult<ChatCompletions> deserializationSuccess:
                        Log.Verbose("[AzureOpenAIService.ChatCompletion] Succeeded to deserialize response body from JSON.");
                        Log.Debug("[AzureOpenAIService.ChatCompletion] Finished to complete chat.");
                        return UncertainResults.Succeed(deserializationSuccess.Result);

                    case IFailureResult<ChatCompletions> deserializationFailure:
                        Log.Error("[AzureOpenAIService.ChatCompletion] Failed to deserialize response body from JSON because -> {0}.",
                            deserializationFailure.Message);
                        return UncertainResults.FailWithTrace<ChatCompletions>(
                            $"Failed to deserialize response body from JSON because -> {deserializationFailure.Message}.");
                    default:
                        Log.Fatal("[AzureOpenAIService.ChatCompletion] Unexpected deserialization result -> {0}", apiResult);
                        throw new ResultPatternMatchException(nameof(serializationResult));
                }
            }
            // Rate limit exceeded
            if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests)
            {
                Log.Error(
                    "[AzureOpenAIService.ChatCompletion] Retryable because the API has exceeded rate limit with status code:({0}){1}, error response:\n{2}.",
                    (int)responseMessage.StatusCode,
                    responseMessage.StatusCode,
                    responseText);
                return new RateLimitExceededResult<ChatCompletions>(
                    $"Retryable because the API has exceeded rate limit with status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:\n{responseText}.");
            }
            // Retryable
            if ((int)responseMessage.StatusCode is >= 500 and <= 599)
            {
                Log.Error(
                    "[AzureOpenAIService.ChatCompletion] Retryable because the API returned status code:({0}){1}, error response:\n{2}.",
                    (int)responseMessage.StatusCode,
                    responseMessage.StatusCode, 
                    responseText);
                return UncertainResults.RetryWithTrace<ChatCompletions>(
                    $"Retryable because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:\n{responseText}.");
            }
            // Response error
            else
            {
                Log.Error(
                    "[AzureOpenAIService.ChatCompletion] Failed because the API returned status code:({0}){1}, error response:\n{2}.",
                    (int)responseMessage.StatusCode,
                    responseMessage.StatusCode, 
                    responseText);
                return UncertainResults.FailWithTrace<ChatCompletions>(
                    $"Failed because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode}, error response:\n{responseText}."
                );
            }
        }
    }
}