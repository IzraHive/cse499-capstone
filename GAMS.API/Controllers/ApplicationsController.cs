using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GAMS.API.Models;
using GAMS.API.Data;
using GAMS.API.Services;
using GAMS.API.DTOs;
namespace GAMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext                 _db;
    private readonly IEmailService                _email;
    private readonly IWorkflowService             _workflow;
    private readonly UserManager<ApplicationUser> _um;

    public ApplicationsController(AppDbContext db, IEmailService email,
        IWorkflowService workflow, UserManager<ApplicationUser> um)
    { _db = db; _email = email; _workflow = workflow; _um = um; }

    // POST /api/applications — submit new application (Applicant only)
    [HttpPost]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Submit([FromBody] SubmitApplicationDto dto)
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var app = new GrantApplication {
            ApplicantId      = user.Id,
            ApplicantName    = user.FullName,
            Parish           = dto.Parish,
            Constituency     = dto.Constituency,
            GrantType        = dto.GrantType,
            GrantDescription = dto.GrantDescription,
            Reason           = dto.Reason,
            Status           = "Submitted"
        };
        _db.GrantApplications.Add(app);
        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLog {
            GrantApplicationId = app.Id,
            UserId     = user.Id,
            UserName   = user.FullName,
            Action     = "Application Submitted",
            PreviousStatus = "",
            NewStatus      = "Submitted"
        });
        await _db.SaveChangesAsync();

        await _email.SendSubmissionConfirmationAsync(
            user.Email!, user.FullName, app.Id,
            app.Parish, app.Constituency, app.GrantType, app.GrantDescription);

        return Ok(new { app.Id, app.Status, message = "Application submitted successfully" });
    }

    // PUT /api/applications/{id}/status — update workflow status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "SocialWorker,Admin,Finance")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var app = await _db.GrantApplications
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var user  = await _um.GetUserAsync(User);
        var roles = await _um.GetRolesAsync(user!);
        var role  = roles.FirstOrDefault() ?? "";

        if (!_workflow.IsValidTransition(app.Status, dto.NewStatus, role))
            return BadRequest(new { message = $"Cannot move from '{app.Status}' to '{dto.NewStatus}' as {role}" });

        var prev   = app.Status;
        app.Status = dto.NewStatus;
        if (dto.Justification != null) app.DecisionJustification = dto.Justification;

        _db.AuditLogs.Add(new AuditLog {
            GrantApplicationId = id,
            UserId     = user!.Id,
            UserName   = user.FullName,
            Action     = $"Status changed to {dto.NewStatus}",
            PreviousStatus = prev,
            NewStatus      = dto.NewStatus
        });
        await _db.SaveChangesAsync();

        var toEmail = app.Applicant?.Email ?? "";
        var toName  = app.Applicant?.FullName ?? "Applicant";
        if (dto.NewStatus is "Approved" or "Declined")
            await _email.SendDecisionAsync(toEmail, toName, id, dto.NewStatus,
                dto.Justification ?? "No justification provided");
        else
            await _email.SendStatusChangeAsync(toEmail, toName, id, dto.NewStatus);

        return Ok(new { app.Id, app.Status });
    }

    // GET /api/applications — list (Applicant sees own, others see all)
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status    = null,
        [FromQuery] string? applicant = null,
        [FromQuery] string? parish    = null,
        [FromQuery] string? dateFrom  = null,
        [FromQuery] string? dateTo    = null)
    {
        var user  = await _um.GetUserAsync(User);
        var roles = await _um.GetRolesAsync(user!);

        IQueryable<GrantApplication> q = _db.GrantApplications
            .Include(a => a.Documents)
            .Include(a => a.AuditLogs);

        if (roles.Contains("Applicant"))
            q = q.Where(a => a.ApplicantId == user!.Id);
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        if (!string.IsNullOrEmpty(applicant))
            q = q.Where(a => a.ApplicantName.ToLower().Contains(applicant.ToLower()));
        if (!string.IsNullOrEmpty(parish))
            q = q.Where(a => a.Parish == parish);
        if (DateTime.TryParse(dateFrom, out var df))
            q = q.Where(a => a.SubmittedAt >= df);
        if (DateTime.TryParse(dateTo, out var dt))
            q = q.Where(a => a.SubmittedAt <= dt.AddDays(1));

        var apps = await q.OrderByDescending(a => a.SubmittedAt).ToListAsync();

        return Ok(apps.Select(a => new {
            a.Id, a.ApplicantId, a.ApplicantName,
            a.Parish, a.Constituency, a.GrantType, a.GrantDescription,
            a.Reason, a.Status, a.SubmittedAt, a.DecisionJustification,
            documentCount = a.Documents.Count,
            auditLogs = a.AuditLogs.OrderBy(l => l.Timestamp).Select(l => new {
                l.Id, l.Action, l.PreviousStatus, l.NewStatus, l.Timestamp, l.UserName
            })
        }));
    }

    // GET /api/applications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var app = await _db.GrantApplications
            .Include(a => a.AuditLogs)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();
        return Ok(new {
            app.Id, app.ApplicantId, app.ApplicantName,
            app.Parish, app.Constituency, app.GrantType, app.GrantDescription,
            app.Reason, app.Status, app.SubmittedAt, app.DecisionJustification,
            documents = app.Documents.Select(d => new {
                d.Id, d.FileName, d.ContentType, d.FileSize, d.UploadedAt, d.UploadedByName
            }),
            auditLogs = app.AuditLogs.OrderBy(l => l.Timestamp).Select(l => new {
                l.Id, l.Action, l.PreviousStatus, l.NewStatus, l.Timestamp, l.UserName
            })
        });
    }

    // GET /api/applications/stats — admin dashboard numbers
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,SocialWorker")]
    public async Task<IActionResult> GetStats()
    {
        var all = await _db.GrantApplications.ToListAsync();
        return Ok(new {
            total         = all.Count,
            submitted     = all.Count(a => a.Status == "Submitted"),
            underReview   = all.Count(a => a.Status == "Under Review"),
            approved      = all.Count(a => a.Status == "Approved"),
            declined      = all.Count(a => a.Status == "Declined"),
            paymentIssued = all.Count(a => a.Status == "Payment Issued"),
            byParish    = all.GroupBy(a => a.Parish)
                             .Select(g => new { parish = g.Key, count = g.Count() }),
            byGrantType = all.GroupBy(a => a.GrantType)
                             .Select(g => new { grantType = g.Key, count = g.Count() }),
            recentByDay = all.Where(a => a.SubmittedAt >= DateTime.UtcNow.AddDays(-30))
                             .GroupBy(a => a.SubmittedAt.Date.ToString("yyyy-MM-dd"))
                             .Select(g => new { date = g.Key, count = g.Count() })
                             .OrderBy(x => x.date)
        });
    }

    // GET /api/applications/auditlog — all audit entries (admin)
    [HttpGet("auditlog")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAuditLog()
    {
        var logs = await _db.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(500)
            .ToListAsync();
        return Ok(logs);
    }
}