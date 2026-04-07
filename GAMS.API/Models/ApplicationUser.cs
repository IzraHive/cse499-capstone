using Microsoft.AspNetCore.Identity;
namespace GAMS.API.Models;
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}