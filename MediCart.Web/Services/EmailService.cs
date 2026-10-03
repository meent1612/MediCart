using System.Net.Http.Json;

namespace MediCart.Web.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }

    // Sends emails through Resend's HTTPS API instead of Gmail SMTP.
    // Reason: Render's free plan blocks SMTP ports (25, 465, 587), but HTTPS (443) is allowed.
    //
    // LIMITATION: without a verified domain, Resend only delivers to the email address
    // that owns the Resend account. Mail to any other address fails with a 403 error.
    public class EmailService : IEmailService
    {
        // Resend's address for sending one email.
        private const string ResendSendUrl = "https://api.resend.com/emails";

        // Resend's shared test sender. Works without any setup.
        // Once you verify your own domain, set the Resend:From setting to an address on it.
        private const string SandboxSender = "MediCart <onboarding@resend.dev>";

        // This text appears inside the email.
        // IMPORTANT: change it whenever the OTP expiry time changes in the backend.
        private const string OtpValidityText = "5 minutes";

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var apiKey = _configuration["Resend:ApiKey"]
                ?? throw new InvalidOperationException("Resend:ApiKey not configured.");

            // Optional setting. If it is missing we use Resend's sandbox sender.
            var fromAddress = _configuration["Resend:From"] ?? SandboxSender;

            var htmlBody = $@"
                <div style='font-family:sans-serif;max-width:420px;margin:0 auto;padding:32px 24px;
                            border:1px solid #e2e8f0;border-radius:10px;'>
                    <h2 style='color:#1a3a2a;margin:0 0 8px;'>MediCart payment OTP</h2>
                    <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>
                        Use this code to confirm your bKash payment.
                        It expires in <strong>{OtpValidityText}</strong>.
                    </p>
                    <div style='background:#f0fdf4;border:1px solid #86efac;border-radius:8px;
                                padding:20px;text-align:center;margin-bottom:24px;'>
                        <span style='font-size:36px;font-weight:700;letter-spacing:10px;
                                     color:#166534;font-family:monospace;'>{otpCode}</span>
                    </div>
                    <p style='color:#94a3b8;font-size:12px;margin:0;'>
                        If you did not request this, ignore this email.
                    </p>
                </div>";

            // The property names below must stay exactly like this: Resend expects them.
            var payload = new
            {
                from = fromAddress,
                to = new[] { toEmail },
                subject = "Your MediCart bKash OTP",
                html = htmlBody
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendSendUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = JsonContent.Create(payload);

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            // Log Resend's reason so it shows up in the Render logs.
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Resend failed to send the OTP email. Status {StatusCode}: {Body}",
                (int)response.StatusCode,
                errorBody);

            throw new InvalidOperationException(
                $"Resend email failed with status {(int)response.StatusCode}.");
        }
    }
}