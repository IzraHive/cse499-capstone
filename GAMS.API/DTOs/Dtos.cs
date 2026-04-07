namespace GAMS.API.DTOs;

public class RegisterDto
{
    public string FullName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Applicant submits for themselves
public class SubmitApplicationDto
{
    public string Parish             { get; set; } = string.Empty;
    public string Constituency       { get; set; } = string.Empty;
    public string GrantType          { get; set; } = string.Empty;
    public string GrantDescription   { get; set; } = string.Empty;
    public string Reason             { get; set; } = string.Empty;
    public string DateOfBirth        { get; set; } = string.Empty;
    public string Gender             { get; set; } = string.Empty;
    public string TRN                { get; set; } = string.Empty;
    public string ContactNumber      { get; set; } = string.Empty;
}

// Staff (Worker/Admin) submits on behalf of an unregistered applicant
public class StaffSubmitApplicationDto
{
    public string ApplicantFullName  { get; set; } = string.Empty;
    public string DateOfBirth        { get; set; } = string.Empty;
    public string Gender             { get; set; } = string.Empty;
    public string TRN                { get; set; } = string.Empty;
    public string ContactNumber      { get; set; } = string.Empty;
    public string Parish             { get; set; } = string.Empty;
    public string Constituency       { get; set; } = string.Empty;
    public string GrantType          { get; set; } = string.Empty;
    public string GrantDescription   { get; set; } = string.Empty;
    public string Reason             { get; set; } = string.Empty;
}

public class UpdateStatusDto
{
    public string NewStatus      { get; set; } = string.Empty;
    public string? Justification { get; set; }
}

public class ResetPasswordDto
{
    public string Email       { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}