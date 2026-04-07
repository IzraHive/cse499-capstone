using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GAMS.API.Models;
using GAMS.API.Data;
using GAMS.API.Services;
using GAMS.API.DTOs;
namespace GAMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser>  _um;
    private readonly SignInManager<ApplicationUser> _sm;

    public AuthController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    {
        _um = um; _sm = sm;
    }

    // POST /api/auth/register
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

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _um.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await _sm.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        // This creates the auth cookie automatically
        await _sm.SignInAsync(user, isPersistent: false);

        var roles = await _um.GetRolesAsync(user);
        return Ok(new {
            fullName = user.FullName,
            email    = user.Email,
            roles    = roles
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _sm.SignOutAsync();
        return Ok(new { message = "Logged out" });
    }

    // GET /api/auth/me — check if logged in and get user info
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user  = await _um.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var roles = await _um.GetRolesAsync(user);
        return Ok(new {
            fullName = user.FullName,
            email    = user.Email,
            roles    = roles,
            userId   = user.Id
        });
    }

    // POST /api/auth/assign-role — admin only, promote users
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
}

public class AssignRoleDto
{
    public string Email { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
}