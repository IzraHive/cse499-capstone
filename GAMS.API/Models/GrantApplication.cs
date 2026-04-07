namespace GAMS.API.Models;
public class GrantApplication
{
    public int Id { get; set; }
    public string ApplicantId { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;

    // New applicant detail fields
    public string ApplicantDateOfBirth { get; set; } = string.Empty;
    public string ApplicantGender { get; set; } = string.Empty;
    public string ApplicantTRN { get; set; } = string.Empty;
    public string ApplicantContactNumber { get; set; } = string.Empty;

    // For staff submissions on behalf of unregistered applicants
    public bool IsStaffSubmission { get; set; } = false;
    public string StaffSubmittedForName { get; set; } = string.Empty;

    public string Parish { get; set; } = string.Empty;
    public string Constituency { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty;
    public string GrantDescription { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? DecisionJustification { get; set; }
    public ApplicationUser? Applicant { get; set; }
    public List<AuditLog> AuditLogs { get; set; } = new();
    public List<Document> Documents { get; set; } = new();
}