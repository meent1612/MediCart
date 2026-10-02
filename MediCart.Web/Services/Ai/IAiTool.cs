using System.Text.Json.Nodes;

namespace MediCart.Web.Services.Ai
{
    // Who is asking. Built on the server from the login cookie, never from the browser.
    public record AiCallContext(string Role, string? UserId);

    // One capability the model may call. Each AI feature is one class implementing this.
    public interface IAiTool
    {
        string Name { get; }
        string Description { get; }

        // JSON-schema object describing the arguments (use a fresh object each time).
        JsonObject ParametersSchema { get; }

        // Roles allowed to use this tool: "Admin", "Customer", "Guest".
        IReadOnlyCollection<string> AllowedRoles { get; }

        // Returns a JSON string with real data for the model to summarise.
        Task<string> ExecuteAsync(JsonObject arguments, AiCallContext context, CancellationToken ct);
    }
}