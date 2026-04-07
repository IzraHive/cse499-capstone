using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GAMS.API.Models;

namespace GAMS.API.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GrantApplication> GrantApplications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Document> Documents { get; set; }
}