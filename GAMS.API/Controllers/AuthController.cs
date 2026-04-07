using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GAMS.API.Models;
using GAMS.API.DTOs;

namespace GAMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser>  _um;
    private readonly SignInManager<ApplicationUser> _sm;

    public AuthController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    { _um = um; _sm = sm; }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email)    ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "All fields are required." });

        var user = new ApplicationUser {
            UserName = dto.Email, Email = dto.Email, FullName = dto.FullName
        };
        var result = await _um.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        await _um.AddToRoleAsync(user, "Applicant");
        return Ok(new { message = "Account created successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _um.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized(new { message = "Invalid email or password." });

        var result = await _sm.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded) return Unauthorized(new { message = "Invalid email or password." });

        await _sm.SignInAsync(user, isPersistent: false);
        var roles = await _um.GetRolesAsync(user);
        return Ok(new { fullName = user.FullName, email = user.Email, roles });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _sm.SignOutAsync();
        return Ok(new { message = "Logged out" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user  = await _um.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var roles = await _um.GetRolesAsync(user);
        return Ok(new { fullName = user.FullName, email = user.Email, roles, userId = user.Id });
    }

    // GET /api/auth/users — list all users (Admin only)
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _um.Users.ToList();
        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _um.GetRolesAsync(u);
            result.Add(new {
                u.Id, u.FullName, u.Email,
                roles,
                role = roles.FirstOrDefault() ?? "None"
            });
        }
        return Ok(result.OrderBy(x => ((dynamic)x).email));
    }

    // POST /api/auth/assign-role — change a user's role (Admin only)
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var user = await _um.FindByEmailAsync(dto.Email);
        if (user == null) return NotFound(new { message = "User not found" });

        var currentRoles = await _um.GetRolesAsync(user);
        await _um.RemoveFromRolesAsync(user, currentRoles);
        await _um.AddToRoleAsync(user, dto.Role);
        return Ok(new { message = $"{user.Email} is now {dto.Role}" });
    }

    // DELETE /api/auth/users/{email} — delete a user (Admin only)
    [HttpDelete("users/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string email)
    {
        var currentUser = await _um.GetUserAsync(User);
        if (currentUser?.Email?.ToLower() == email.ToLower())
            return BadRequest(new { message = "You cannot delete your own account." });

        var user = await _um.FindByEmailAsync(email);
        if (user == null) return NotFound(new { message = "User not found" });

        var result = await _um.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return Ok(new { message = $"User {email} deleted successfully" });
    }

    // POST /api/auth/reset-password — reset a user's password (Admin only)
    [HttpPost("reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _um.FindByEmailAsync(dto.Email);
        if (user == null) return NotFound(new { message = "User not found" });

        var token  = await _um.GeneratePasswordResetTokenAsync(user);
        var result = await _um.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return Ok(new { message = $"Password reset for {dto.Email}" });
    }
}

public class AssignRoleDto
{
    public string Email { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
}