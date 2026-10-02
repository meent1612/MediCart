using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    // Body sent by wwwroot/js/chatbot.js to POST /api/chat/message.
    // The "role" field the browser sends is deliberately NOT bound:
    // the server decides the role from the login cookie.
    public class ChatMessageRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public List<ChatHistoryItem>? History { get; set; }
    }

    public class ChatHistoryItem
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}