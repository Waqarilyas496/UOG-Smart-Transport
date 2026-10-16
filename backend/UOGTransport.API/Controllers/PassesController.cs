using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;
using UOGTransport.API.Enums;
using UOGTransport.API.Models;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PassesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PassesController(AppDbContext db)
    {
        _db = db;
    }

    // PostgreSQL timestamptz only accepts UTC DateTimes
    private static DateTime AsUtc(DateTime d) =>
        d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc);

    private static PassResponse ToResponse(TransportPass p) => new()
    {
        Id = p.Id,
        PassNumber = p.PassNumber,
        StudentId = p.StudentId,
        StudentName = p.Student?.User?.FullName ?? string.Empty,
        RollNumber = p.Student?.RollNumber ?? string.Empty,
        RouteId = p.RouteId,
        RouteName = p.Route?.RouteName ?? string.Empty,
        ValidFrom = p.ValidFrom,
        ValidUntil = p.ValidUntil,
        Status = p.Status == PassStatus.Active && p.ValidUntil < DateTime.UtcNow
            ? PassStatus.Expired.ToString()
            : p.Status.ToString()
    };

    // POST /api/passes - admin issues a pass for a student (uses the student's assigned route)
    [HttpPost]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<PassResponse>> Issue(IssuePassRequest request)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == request.StudentId);

        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        if (student.RouteId == null)
        {
            return BadRequest(new { message = "Assign a route to this student first." });
        }

        var from = AsUtc(request.ValidFrom);
        var until = AsUtc(request.ValidUntil);
        if (until <= from)
        {
            return BadRequest(new { message = "ValidUntil must be after ValidFrom." });
        }

        var now = DateTime.UtcNow;
        var hasActive = await _db.TransportPasses.AnyAsync(p =>
            p.StudentId == student.Id && p.Status == PassStatus.Active && p.ValidUntil >= now);

        if (hasActive)
        {
            return BadRequest(new { message = "This student already has an active pass." });
        }

        var pass = new TransportPass
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RouteId = student.RouteId.Value,
            PassNumber = "UOG-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            ValidFrom = from,
            ValidUntil = until,
            Status = PassStatus.Active,
            IssuedAt = now
        };

        _db.TransportPasses.Add(pass);
        await _db.SaveChangesAsync();

        pass.Student = student;
        pass.Route = student.Route;

        return Ok(ToResponse(pass));
    }

    // GET /api/passes - admin sees all passes
    [HttpGet]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<List<PassResponse>>> GetAll()
    {
        var passes = await _db.TransportPasses
            .Include(p => p.Student).ThenInclude(s => s!.User)
            .Include(p => p.Route)
            .OrderByDescending(p => p.IssuedAt)
            .ToListAsync();

        return Ok(passes.Select(ToResponse).ToList());
    }

    // GET /api/passes/me - a student's own currently valid pass
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<PassResponse>> GetMine()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found for this account." });
        }

        var now = DateTime.UtcNow;
        var pass = await _db.TransportPasses
            .Include(p => p.Student).ThenInclude(s => s!.User)
            .Include(p => p.Route)
            .Where(p => p.StudentId == student.Id
                        && p.Status == PassStatus.Active
                        && p.ValidFrom <= now
                        && p.ValidUntil >= now)
            .OrderByDescending(p => p.IssuedAt)
            .FirstOrDefaultAsync();

        if (pass == null)
        {
            return NotFound(new { message = "You have no valid transport pass right now." });
        }

        return Ok(ToResponse(pass));
    }
}