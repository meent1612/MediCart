using System.Security.Claims;
using MediCart.Web.Models;
using MediCart.Web.Services.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MediCart.Web.Controllers
{
    // Endpoint used by the Baymax widget (wwwroot/js/chatbot.js).
    // Open to everyone, but what each role can do is decided server-side.
    [ApiController]
    [Route("api/chat")]
    public class AiChatController : ControllerBase
    {
        // Roles that have AI tools wired up. Add "Customer" / "Guest" here
        // once their tools exist. Others get 501 and the widget keeps
        // using its scripted replies.
        private static readonly HashSet<string> AiEnabledRoles = new() { "Admin" };

        private readonly IAiChatService _chat;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(IAiChatService chat, ILogger<AiChatController> logger)
        {
            _chat = chat;
            _logger = logger;
        }

        [HttpPost("message")]
        [EnableRateLimiting("ai-chat")]
        public async Task<IActionResult> Message([FromBody] ChatMessageRequest request, CancellationToken ct)
        {
            // CSRF guard: a cross-site page cannot set this header without a CORS
            // preflight, which this app does not allow. chatbot.js sends it.
            if (!string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal))
            {
                return BadRequest(new { error = "Missing required header." });
            }

            var role = ResolveRole();

            if (!AiEnabledRoles.Contains(role))
            {
                return StatusCode(StatusCodes.Status501NotImplemented,
                    new { error = "AI assistant is not enabled for this account type yet." });
            }

            var history = (request.History ?? new List<ChatHistoryItem>())
                .Select(h => new AiHistoryTurn(h.Sender ?? string.Empty, h.Text ?? string.Empty))
                .ToList();

            var context = new AiCallContext(role, User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var reply = await _chat.ReplyAsync(request.Message, history, context, ct);
                return Ok(new { reply });
            }
            catch (GroqApiException ex)
            {
                _logger.LogError(ex, "Groq call failed for role {Role}.", role);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { error = "The AI service is unavailable right now." });
            }
        }

        private string ResolveRole()
        {
            if (User.IsInRole("Admin")) return "Admin";
            if (User.IsInRole("Customer")) return "Customer";
            return "Guest";
        }
    }
}