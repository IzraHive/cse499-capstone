using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using GAMS.API.Services;
namespace GAMS.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string html)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Mailtrap:FromName"]!,
            _config["Mailtrap:FromEmail"]!));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Mailtrap:Host"],
            int.Parse(_config["Mailtrap:Port"]!),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _config["Mailtrap:Username"],
            _config["Mailtrap:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendSubmissionConfirmationAsync(string toEmail, string toName,
        int appId, string parish, string constituency, string grantType, string grantDescription)
    {
        var subject = $"GAMS - Application #{appId} Successfully Submitted";
        var html = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px;text-align:center'>
            <h1 style='color:#fff;margin:0'>GAMS</h1>
            <p style='color:#a8c4e0;margin:4px 0 0;font-size:13px'>Ministry of Labour &amp; Social Security</p>
          </div>
          <div style='padding:24px;background:#fff'>
            <p>Dear {toName},</p>
            <p>Your grant application has been <strong>successfully submitted</strong>.</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0'>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Application #</td><td style='padding:8px;border:1px solid #d0e8f8'>{appId}</td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Parish</td><td style='padding:8px;border:1px solid #d0e8f8'>{parish}</td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Constituency</td><td style='padding:8px;border:1px solid #d0e8f8'>{constituency}</td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Grant Type</td><td style='padding:8px;border:1px solid #d0e8f8'>{grantType}</td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Description</td><td style='padding:8px;border:1px solid #d0e8f8'>{grantDescription}</td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;color:#1a3a5c;border:1px solid #d0e8f8'>Status</td><td style='padding:8px;border:1px solid #d0e8f8'>Submitted</td></tr>
            </table>
            <p style='color:#555;font-size:13px'>You will be notified as your application progresses.</p>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | GAMS</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }

    public async Task SendStatusChangeAsync(string toEmail, string toName, int appId, string newStatus)
    {
        var subject = $"GAMS - Application #{appId} Status Updated: {newStatus}";
        var html = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px;text-align:center'>
            <h1 style='color:#fff;margin:0'>GAMS</h1>
          </div>
          <div style='padding:24px;background:#fff'>
            <p>Dear {toName},</p>
            <p>Application <strong>#{appId}</strong> status has been updated to: <strong style='color:#1a3a5c'>{newStatus}</strong></p>
            <p style='color:#555;font-size:13px'>You will be notified when a final decision is made.</p>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | GAMS</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }

    public async Task SendDecisionAsync(string toEmail, string toName, int appId,
        string decision, string justification)
    {
        var color   = decision == "Approved" ? "#1e5c2e" : "#8b0000";
        var subject = $"GAMS - Application #{appId} Final Decision: {decision}";
        var html = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden'>
          <div style='background:#1a3a5c;padding:20px;text-align:center'>
            <h1 style='color:#fff;margin:0'>GAMS</h1>
          </div>
          <div style='background:{color};padding:12px;text-align:center'>
            <h2 style='color:#fff;margin:0'>Final Decision: {decision}</h2>
          </div>
          <div style='padding:24px;background:#fff'>
            <p>Dear {toName},</p>
            <p>A final decision has been made on Application <strong>#{appId}</strong>.</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0'>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;border:1px solid #d0e8f8'>Decision</td><td style='padding:8px;border:1px solid #d0e8f8;color:{color}'><strong>{decision}</strong></td></tr>
              <tr><td style='padding:8px;background:#f0f7ff;font-weight:bold;border:1px solid #d0e8f8'>Justification</td><td style='padding:8px;border:1px solid #d0e8f8'>{justification}</td></tr>
            </table>
          </div>
          <div style='background:#f5f5f5;padding:12px;text-align:center;font-size:12px;color:#888'>Ministry of Labour &amp; Social Security | GAMS</div>
        </div>";
        await SendAsync(toEmail, toName, subject, html);
    }
}