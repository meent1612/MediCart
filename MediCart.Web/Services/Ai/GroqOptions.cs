namespace MediCart.Web.Services.Ai
{
    // Bound to the "Groq" configuration section.
    // ApiKey comes from user-secrets (Groq:ApiKey) and is never committed.
    public class GroqOptions
    {
        public const string SectionName = "Groq";

        public string ApiKey { get; set; } = string.Empty;

        // Override with user-secrets "Groq:Model" if this model is retired.
        public string Model { get; set; } = "llama-3.3-70b-versatile";

        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";

        public int TimeoutSeconds { get; set; } = 30;
    }
}