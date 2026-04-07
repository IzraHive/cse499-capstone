namespace GAMS.API.Models;
public class AuditLog
{
    public int Id { get; set; }
    public int GrantApplicationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public GrantApplication? GrantApplication { get; set; }
}