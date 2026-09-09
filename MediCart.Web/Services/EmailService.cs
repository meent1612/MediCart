using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MediCart.Web.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var fromEmail = _configuration["Email:From"]
                ?? throw new InvalidOperationException("Email:From not configured.");
            var appPassword = _configuration["Email:AppPassword"]
                ?? throw new InvalidOperationException("Email:AppPassword not configured.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MediCart", fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your MediCart bKash OTP";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family:sans-serif;max-width:420px;margin:0 auto;padding:32px 24px;
                                border:1px solid #e2e8f0;border-radius:10px;'>
                        <h2 style='color:#1a3a2a;margin:0 0 8px;'>MediCart payment OTP</h2>
                        <p style='color:#64748b;margin:0 0 24px;font-size:14px;'>
                            Use this code to confirm your bKash payment.
                            It expires in <strong>30 seconds</strong>.
                        </p>
                        <div style='background:#f0fdf4;border:1px solid #86efac;border-radius:8px;
                                    padding:20px;text-align:center;margin-bottom:24px;'>
                            <span style='font-size:36px;font-weight:700;letter-spacing:10px;
                                         color:#166534;font-family:monospace;'>{otpCode}</span>
                        </div>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>
                            If you did not request this, ignore this email.
                        </p>
                    </div>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(fromEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}