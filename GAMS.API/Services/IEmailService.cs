namespace GAMS.API.Services;
public interface IEmailService
{
    Task SendSubmissionConfirmationAsync(string toEmail, string toName, int appId,
        string parish, string constituency, string grantType, string grantDescription);
    Task SendStatusChangeAsync(string toEmail, string toName, int appId, string newStatus);
    Task SendDecisionAsync(string toEmail, string toName, int appId,
        string decision, string justification);
}