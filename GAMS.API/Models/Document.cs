namespace GAMS.API.Models;
public class Document
{
    public int Id { get; set; }
    public int GrantApplicationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public GrantApplication? GrantApplication { get; set; }
}