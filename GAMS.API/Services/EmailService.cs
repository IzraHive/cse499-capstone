using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using GAMS.API.Services;
namespace GAMS.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) { _config = config; }

    private async Task SendAsync(string toEmail, string toName, string subject, string html)
    {
        try
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_config["Mailtrap:FromName"]!, _config["Mailtrap:FromEmail"]!));
            msg.To.Add(new MailboxAddress(toName, toEmail));
            msg.Subject = subject;
            msg.Body    = new TextPart("html") { Text = html };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Mailtrap:Host"], int.Parse(_config["Mailtrap:Port"]!), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Mailtrap:Username"], _config["Mailtrap:Password"]);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}");
            // Email failure never crashes the application
        }
    }

    public async Task SendSubmissionConfirmationAsync(string toEmail, string toName,
        int appId, string parish, string constituency, string grantType, string grantDescription)
    {
        var subject = $"GAMS — Application #{appId} Submitted Successfully";
        var html = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #ddd;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px 28px;text-align:center'>
            <h1 style='color:#fff;margin:0;font-size:22px;letter-spacing:2px'>GAMS</h1>
            <p style='color:#a8c4e0;margin:4px 0 0;font-size:13px'>Ministry of Labour &amp; Social Security</p>
          </div>
          <div style='background:#e8f4fd;padding:14px 28px;border-bottom:1px solid #bee3f8'>
            <h2 style='color:#1a3a5c;margin:0;font-size:15px'>&#10003; Application Received</h2>
          </div>
          <div style='padding:24px 28px;background:#fff'>
            <p style='color:#333'>Dear {toName},</p>
            <p style='color:#333'>Your grant application has been <strong>successfully submitted</strong> and is now being processed.</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0'>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;width:38%;border:1px solid #d0e8f8'>Application #</td><td style='padding:9px 12px;border:1px solid #d0e8f8'>{appId}</td></tr>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Parish</td><td style='padding:9px 12px;border:1px solid #d0e8f8'>{parish}</td></tr>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Constituency</td><td style='padding:9px 12px;border:1px solid #d0e8f8'>{constituency}</td></tr>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Grant Type</td><td style='padding:9px 12px;border:1px solid #d0e8f8'>{grantType}</td></tr>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Description</td><td style='padding:9px 12px;border:1px solid #d0e8f8'>{grantDescription}</td></tr>
            </table>
            <p style='color:#555;font-size:13px'>You will receive email notifications as your application progresses. Please keep your Application # for reference.</p>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | Grant Application Management System</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }

    public async Task SendStatusChangeAsync(string toEmail, string toName, int appId, string newStatus)
    {
        var subject = $"GAMS — Application #{appId} Status Updated: {newStatus}";
        var html = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #ddd;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px 28px;text-align:center'>
            <h1 style='color:#fff;margin:0;font-size:22px;letter-spacing:2px'>GAMS</h1>
          </div>
          <div style='background:#fff8e1;padding:14px 28px;border-bottom:1px solid #ffe082'>
            <h2 style='color:#7f5000;margin:0;font-size:15px'>Application Status Updated</h2>
          </div>
          <div style='padding:24px 28px;background:#fff'>
            <p style='color:#333'>Dear {toName},</p>
            <p style='color:#333'>Your grant application <strong>#{appId}</strong> status has been updated to:</p>
            <div style='background:#f0f7ff;border-left:4px solid #1a3a5c;padding:12px 16px;margin:16px 0;border-radius:4px'>
              <strong style='color:#1a3a5c;font-size:15px'>{newStatus}</strong>
            </div>
            <p style='color:#555;font-size:13px'>You will receive another notification when a final decision is made.</p>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | Grant Application Management System</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }

    public async Task SendDecisionAsync(string toEmail, string toName, int appId,
        string decision, string justification)
    {
        var approved    = decision == "Approved";
        var headerColor = approved ? "#1e5c2e" : "#8b0000";
        var subject     = $"GAMS — Application #{appId} Final Decision: {decision}";
        var html = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #ddd;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px 28px;text-align:center'>
            <h1 style='color:#fff;margin:0;font-size:22px;letter-spacing:2px'>GAMS</h1>
          </div>
          <div style='background:{headerColor};padding:14px 28px;text-align:center'>
            <h2 style='color:#fff;margin:0;font-size:17px'>{(approved ? "✓" : "✗")} Final Decision: {decision}</h2>
          </div>
          <div style='padding:24px 28px;background:#fff'>
            <p style='color:#333'>Dear {toName},</p>
            <p style='color:#333'>A final decision has been made on your grant application <strong>#{appId}</strong>.</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0'>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;width:38%;border:1px solid #d0e8f8'>Decision</td>
                  <td style='padding:9px 12px;border:1px solid #d0e8f8;color:{headerColor};font-weight:bold'>{decision}</td></tr>
              <tr><td style='padding:9px 12px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Justification</td>
                  <td style='padding:9px 12px;border:1px solid #d0e8f8'>{justification}</td></tr>
            </table>
            <p style='color:#555;font-size:13px'>For queries, please contact the Ministry of Labour &amp; Social Security.</p>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | Grant Application Management System</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }
}