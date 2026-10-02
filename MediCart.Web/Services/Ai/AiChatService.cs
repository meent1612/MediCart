using System.Text.Json;
using System.Text.Json.Nodes;

namespace MediCart.Web.Services.Ai
{
    // One earlier chat turn as sent by the browser. Untrusted input.
    public record AiHistoryTurn(string Sender, string Text);

    public interface IAiChatService
    {
        Task<string> ReplyAsync(
            string message,
            IReadOnlyList<AiHistoryTurn> history,
            AiCallContext context,
            CancellationToken ct = default);
    }

    public class AiChatService : IAiChatService
    {
        private const int MaxToolRounds = 3;
        private const int MaxHistoryTurns = 8;
        private const int MaxTurnChars = 500;

        private const string FallbackReply =
            "Sorry, I couldn't put an answer together. Please try again.";

        private readonly IGroqClient _groq;
        private readonly IEnumerable<IAiTool> _tools;
        private readonly ILogger<AiChatService> _logger;

        public AiChatService(IGroqClient groq, IEnumerable<IAiTool> tools, ILogger<AiChatService> logger)
        {
            _groq = groq;
            _tools = tools;
            _logger = logger;
        }

        public async Task<string> ReplyAsync(
            string message,
            IReadOnlyList<AiHistoryTurn> history,
            AiCallContext context,
            CancellationToken ct = default)
        {
            // Role enforcement: the model is only ever shown tools this role may use,
            // and only these tools can be executed below.
            var allowedTools = _tools
                .Where(t => t.AllowedRoles.Contains(context.Role))
                .ToList();

            
var toolDefinitions = new JsonArray();

foreach (var tool in allowedTools)
{
    toolDefinitions.Add(new JsonObject
    {
        ["type"] = "function",
        ["function"] = new JsonObject
        {
            ["name"] = tool.Name,
            ["description"] = tool.Description,
            ["parameters"] = tool.ParametersSchema.DeepClone()
        }
    });
}

            var messages = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "system",
                    ["content"] = AiPrompts.ForRole(context.Role)
                }
            };
            AppendHistory(messages, message, history);

            for (int round = 0; round <= MaxToolRounds; round++)
            {
                // On the last round no tools are offered, forcing a written answer.
                var toolsForRound = round < MaxToolRounds ? toolDefinitions : null;

                var assistant = await _groq.CompleteAsync(messages, toolsForRound, ct);

                var toolCalls = assistant["tool_calls"] as JsonArray;
                if (toolCalls == null || toolCalls.Count == 0)
                {
                    var text = assistant["content"]?.GetValue<string>()?.Trim();
                    return string.IsNullOrEmpty(text) ? FallbackReply : text;
                }

                if (round == MaxToolRounds)
                    break;

                messages.Add(assistant);

                foreach (var call in toolCalls)
                {
                    var callId = call?["id"]?.GetValue<string>() ?? string.Empty;
                    var name = call?["function"]?["name"]?.GetValue<string>() ?? string.Empty;
                    var rawArguments = call?["function"]?["arguments"]?.GetValue<string>();

                    var result = await RunToolAsync(allowedTools, name, rawArguments, context, ct);

                    messages.Add(new JsonObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = callId,
                        ["content"] = result
                    });
                }
            }

            return FallbackReply;
        }

        private async Task<string> RunToolAsync(
            List<IAiTool> allowedTools,
            string name,
            string? rawArguments,
            AiCallContext context,
            CancellationToken ct)
        {
            var tool = allowedTools.FirstOrDefault(t => t.Name == name);
            if (tool == null)
            {
                _logger.LogWarning("Model asked for unknown or not-permitted tool '{Tool}' (role {Role}).", name, context.Role);
                return JsonSerializer.Serialize(new { error = "That tool is not available." });
            }

            try
            {
                var arguments = JsonNode.Parse(string.IsNullOrWhiteSpace(rawArguments) ? "{}" : rawArguments)
                                as JsonObject ?? new JsonObject();

                return await tool.ExecuteAsync(arguments, context, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "AI tool '{Tool}' failed.", name);
                return JsonSerializer.Serialize(new { error = "The data lookup failed." });
            }
        }

        private static void AppendHistory(JsonArray messages, string message, IReadOnlyList<AiHistoryTurn> history)
        {
            var turns = history
                .Where(h => !string.IsNullOrWhiteSpace(h.Text))
                .TakeLast(MaxHistoryTurns)
                .ToList();

            // chatbot.js already includes the newest user message as the last turn.
            // Drop it so the validated copy is added exactly once below.
            if (turns.Count > 0
                && turns[^1].Sender == "user"
                && turns[^1].Text.Trim() == message.Trim())
            {
                turns.RemoveAt(turns.Count - 1);
            }

            foreach (var turn in turns)
            {
                messages.Add(new JsonObject
                {
                    ["role"] = turn.Sender == "user" ? "user" : "assistant",
                    ["content"] = Limit(turn.Text)
                });
            }

            messages.Add(new JsonObject
            {
                ["role"] = "user",
                ["content"] = Limit(message)
            });
        }

        private static string Limit(string text)
        {
            var trimmed = text.Trim();
            return trimmed.Length <= MaxTurnChars ? trimmed : trimmed[..MaxTurnChars];
        }
    }
}