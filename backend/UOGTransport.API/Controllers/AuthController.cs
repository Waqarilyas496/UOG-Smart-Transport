using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;
using UOGTransport.API.Enums;
using UOGTransport.API.Models;
using UOGTransport.API.Services;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email is already registered." });
        }

        if (request.Role == UserRole.Student &&
            (string.IsNullOrWhiteSpace(request.RollNumber) || string.IsNullOrWhiteSpace(request.Batch)))
        {
            return BadRequest(new { message = "RollNumber and Batch are required for Student role." });
        }

        if (request.Role == UserRole.Driver && string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            return BadRequest(new { message = "LicenseNumber is required for Driver role." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            PhoneNumber = request.PhoneNumber
        };

        _db.Users.Add(user);

        if (request.Role == UserRole.Student)
        {
            _db.Students.Add(new Student
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RollNumber = request.RollNumber!,
                Batch = request.Batch!,
                Department = request.Department ?? string.Empty,
                Semester = request.Semester ?? 0
            });
        }
        else if (request.Role == UserRole.Driver)
        {
            _db.Drivers.Add(new Driver
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = request.LicenseNumber!
            });
        }

        await _db.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "This account has been disabled." });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { userId, email, role, message = "Token is valid!" });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "You are an admin! This endpoint is restricted." });
    }
}