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

    public interface IGroqClient
    {
        // Sends one chat-completions request and returns the assistant "message" object
        // (it may contain "content" and/or "tool_calls").
        Task<JsonObject> CompleteAsync(JsonArray messages, JsonArray? tools, CancellationToken ct = default);
    }

    public class GroqClient : IGroqClient
    {
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
    ["max_tokens"] = 700
};

if (tools != null && tools.Count > 0)
{
    body["tools"] = tools.DeepClone();
    body["tool_choice"] = "auto";
}
            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
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

                // DeepClone detaches the node from the response tree so it can be
                // added to our messages array in the next round.
                return (JsonObject)message.DeepClone();
            }
        }
    }
}