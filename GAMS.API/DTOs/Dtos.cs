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

public class SubmitApplicationDto
{
    public string Parish           { get; set; } = string.Empty;
    public string Constituency     { get; set; } = string.Empty;
    public string GrantType        { get; set; } = string.Empty;
    public string GrantDescription { get; set; } = string.Empty;
    public string Reason           { get; set; } = string.Empty;
}

public class UpdateStatusDto
{
    public string NewStatus      { get; set; } = string.Empty;
    public string? Justification { get; set; }
}