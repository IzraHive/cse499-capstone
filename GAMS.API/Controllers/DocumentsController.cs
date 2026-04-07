using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GAMS.API.Models;
using GAMS.API.Data;

namespace GAMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext                 _db;
    private readonly UserManager<ApplicationUser> _um;

    private static readonly string[] AllowedTypes = {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg", "image/jpg", "image/png", "image/gif"
    };
    private const long MaxBytes = 10 * 1024 * 1024;

    public DocumentsController(AppDbContext db, UserManager<ApplicationUser> um)
    { _db = db; _um = um; }

    // POST /api/documents/upload/{appId} — Applicant uploads to own application
    [HttpPost("upload/{applicationId}")]
    [Authorize(Roles = "Applicant")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int applicationId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });
        if (file.Length > MaxBytes)
            return BadRequest(new { message = "File exceeds 10 MB limit." });
        if (!AllowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "File type not allowed. Use PDF, Word, JPG, or PNG." });

        var user = await _um.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var app = await _db.GrantApplications.FindAsync(applicationId);
        if (app == null) return NotFound(new { message = "Application not found." });
        if (app.ApplicantId != user.Id) return Forbid();

        return await SaveDocument(applicationId, file, user.Id, user.FullName, app.Status);
    }

    // POST /api/documents/staff-upload/{appId} — Admin or SocialWorker uploads to any application
    [HttpPost("staff-upload/{applicationId}")]
    [Authorize(Roles = "Admin,SocialWorker")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> StaffUpload(int applicationId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });
        if (file.Length > MaxBytes)
            return BadRequest(new { message = "File exceeds 10 MB limit." });
        if (!AllowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "File type not allowed. Use PDF, Word, JPG, or PNG." });

        var user = await _um.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var app = await _db.GrantApplications.FindAsync(applicationId);
        if (app == null) return NotFound(new { message = "Application not found." });

        return await SaveDocument(applicationId, file, user.Id, user.FullName, app.Status);
    }

    // Shared save logic
    private async Task<IActionResult> SaveDocument(int applicationId, IFormFile file,
        string userId, string userName, string appStatus)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var doc = new Document {
            GrantApplicationId = applicationId,
            FileName           = Path.GetFileName(file.FileName),
            ContentType        = file.ContentType,
            FileData           = ms.ToArray(),
            FileSize           = file.Length,
            UploadedByUserId   = userId,
            UploadedByName     = userName
        };
        _db.Documents.Add(doc);

        _db.AuditLogs.Add(new AuditLog {
            GrantApplicationId = applicationId,
            UserId             = userId,
            UserName           = userName,
            Action             = $"Document uploaded: {doc.FileName}",
            PreviousStatus     = appStatus,
            NewStatus          = appStatus
        });

        await _db.SaveChangesAsync();
        return Ok(new { doc.Id, doc.FileName, doc.FileSize, doc.UploadedAt });
    }

    // GET /api/documents/{id} — download
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var user  = await _um.GetUserAsync(User);
        var roles = await _um.GetRolesAsync(user!);

        var doc = await _db.Documents
            .Include(d => d.GrantApplication)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        if (roles.Contains("Applicant") && doc.GrantApplication?.ApplicantId != user!.Id)
            return Forbid();

        return File(doc.FileData, doc.ContentType, doc.FileName);
    }

    // GET /api/documents/application/{appId} — list
    [HttpGet("application/{applicationId}")]
    public async Task<IActionResult> ListForApplication(int applicationId)
    {
        var user  = await _um.GetUserAsync(User);
        var roles = await _um.GetRolesAsync(user!);

        var app = await _db.GrantApplications.FindAsync(applicationId);
        if (app == null) return NotFound();
        if (roles.Contains("Applicant") && app.ApplicantId != user!.Id) return Forbid();

        var docs = await _db.Documents
            .Where(d => d.GrantApplicationId == applicationId)
            .Select(d => new { d.Id, d.FileName, d.ContentType, d.FileSize, d.UploadedAt, d.UploadedByName })
            .ToListAsync();

        return Ok(docs);
    }

    // DELETE /api/documents/{id} — Admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Document deleted" });
    }
}