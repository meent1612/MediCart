using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace MediCart.Web.Services.Ai
{
    public class GroqApiException : Exception
    {
        public GroqApiException(string message, Exception? inner = null) : base(message, inner) { }
    }

    // Thrown when Groq still answers 429 after one retry.
    public class GroqRateLimitException : GroqApiException
    {
        public GroqRateLimitException(string message) : base(message) { }
    }

    public interface IGroqClient
    {
        // Sends one chat-completions request and returns the assistant "message" object
        // (it may contain "content" and/or "tool_calls").
        Task<JsonObject> CompleteAsync(JsonArray messages, JsonArray? tools, CancellationToken ct = default);
    }

    public class GroqClient : IGroqClient
    {
        private const int MaxRetryWaitSeconds = 5;

        private readonly HttpClient _http;
        private readonly GroqOptions _options;
        private readonly ILogger<GroqClient> _logger;

        public GroqClient(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqClient> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;

            _http.BaseAddress = new Uri(_options.BaseUrl);
            _http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public async Task<JsonObject> CompleteAsync(JsonArray messages, JsonArray? tools, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new GroqApiException("Groq API key is not configured (set Groq:ApiKey in user-secrets).");

            var body = new JsonObject
            {
                ["model"] = _options.Model,
                ["messages"] = messages.DeepClone(),
                ["temperature"] = 0.2,
                ["max_tokens"] = 500
            };

            if (tools != null && tools.Count > 0)
            {
                body["tools"] = tools.DeepClone();
                body["tool_choice"] = "auto";
            }

            var json = body.ToJsonString();

            // Attempt 0 = normal call, attempt 1 = single retry after a 429.
            for (int attempt = 0; attempt <= 1; attempt++)
            {
                // A request message cannot be reused, so build a new one per attempt.
                using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                HttpResponseMessage response;
                try
                {
                    response = await _http.SendAsync(request, ct);
                }
                catch (Exception ex) when (ex is HttpRequestException
                                           || (ex is TaskCanceledException && !ct.IsCancellationRequested))
                {
                    _logger.LogError(ex, "Could not reach Groq.");
                    throw new GroqApiException("Could not reach Groq.", ex);
                }

                using (response)
                {
                    var text = await response.Content.ReadAsStringAsync(ct);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("Groq returned 429: {Body}", text);

                        var waitSeconds = GetRetryAfterSeconds(response);

                        if (attempt == 0 && waitSeconds <= MaxRetryWaitSeconds)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Math.Max(waitSeconds, 1)), ct);
                            continue;
                        }

                        throw new GroqRateLimitException("Groq rate limit reached.");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Groq returned {Status}: {Body}", (int)response.StatusCode, text);
                        throw new GroqApiException($"Groq returned status {(int)response.StatusCode}.");
                    }

                    var root = JsonNode.Parse(text) as JsonObject;
                    var message = root?["choices"]?[0]?["message"] as JsonObject;

                    if (message == null)
                    {
                        _logger.LogWarning("Unexpected Groq response shape: {Body}", text);
                        throw new GroqApiException("Groq returned an unexpected response.");
                    }

                    var usage = root?["usage"];
                    if (usage != null)
                    {
                        int? promptTokens = usage["prompt_tokens"] is JsonValue pv && pv.TryGetValue<int>(out var pt) ? pt : null;
                        int? completionTokens = usage["completion_tokens"] is JsonValue cv && cv.TryGetValue<int>(out var ctVal) ? ctVal : null;
                        int? totalTokens = usage["total_tokens"] is JsonValue tv && tv.TryGetValue<int>(out var tt) ? tt : null;

                        _logger.LogInformation(
                            "Groq token usage: prompt={PromptTokens}, completion={CompletionTokens}, total={TotalTokens}",
                            promptTokens,
                            completionTokens,
                            totalTokens);
                    }

                    // DeepClone detaches the node from the response tree so it can be
                    // added to our messages array in the next round.
                    return (JsonObject)message.DeepClone();
                }
            }

            throw new GroqRateLimitException("Groq rate limit reached.");
        }

        // Groq sends Retry-After in seconds. If missing, return a value that skips the retry.
        private static double GetRetryAfterSeconds(HttpResponseMessage response)
        {
            var delta = response.Headers.RetryAfter?.Delta;
            return delta.HasValue ? delta.Value.TotalSeconds : MaxRetryWaitSeconds + 1;
        }
    }
}