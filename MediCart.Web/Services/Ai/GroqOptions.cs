namespace MediCart.Web.Services.Ai
{
    // Bound to the "Groq" configuration section.
    // ApiKey comes from user-secrets (Groq:ApiKey) locally and from the
    // Groq__ApiKey environment variable on the host. It is never committed.
    public class GroqOptions
    {
        public const string SectionName = "Groq";

        public string ApiKey { get; set; } = string.Empty;

        // Default matches the model the chatbot was tested with.
        // Override with Groq:Model (user-secrets) or Groq__Model (host) if it is retired.
        public string Model { get; set; } = "openai/gpt-oss-20b";

        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";

        public int TimeoutSeconds { get; set; } = 30;
    }
}