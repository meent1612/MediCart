using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediCart.Web.Models;
using MediCart.Web.Services.Ai;

namespace MediCart.Web.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class AiChatController : ControllerBase
    {
        private static readonly HashSet<string> AiEnabledRoles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Admin",
                "Customer",
                "Guest"
            };

        private readonly IAiChatService _chat;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(
            IAiChatService chat,
            ILogger<AiChatController> logger)
        {
            _chat = chat;
            _logger = logger;
        }

        [HttpPost("message")]
        [EnableRateLimiting("ai-chat")]
        public async Task<IActionResult> Message(
            [FromBody] ChatMessageRequest request,
            CancellationToken ct)
        {
            if (!Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                || !string.Equals(
                    requestedWith.ToString(),
                    "XMLHttpRequest",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    error = "Invalid chat request."
                });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    error = "Message is required."
                });
            }

            var role =
                User.IsInRole("Admin")
                    ? "Admin"
                    : User.IsInRole("Customer")
                        ? "Customer"
                        : "Guest";

            if (!AiEnabledRoles.Contains(role))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "This assistant is not available for your account."
                });
            }

            var history = request.History?
                .Where(h => h != null && !string.IsNullOrWhiteSpace(h.Text))
                .Select(h => new AiHistoryTurn(
                    h.Sender,
                    h.Text))
                .ToList()
                ?? new List<AiHistoryTurn>();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var context = new AiCallContext(
                role,
                userId);

            try
            {
                var reply = await _chat.ReplyAsync(
                    request.Message,
                    history,
                    context,
                    ct);

                return Ok(new
                {
                    reply
                });
            }
            catch (GroqRateLimitException ex)
            {
                _logger.LogWarning(ex, "AI chat rate limited for role {Role}.", role);

                return Ok(new
                {
                    reply = "Baymax is busy right now. Please try again in a minute."
                });
            }
            catch (GroqApiException ex)
            {
                // Full detail goes to the server log only (missing key, 429, timeout...).
                // The browser gets a generic message and never sees config hints.
                _logger.LogError(ex, "AI chat failed for role {Role}.", role);

                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    error = "The assistant is unavailable right now. Please try again in a moment."
                });
            }
        }
    }
}