using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/students - admins only (contains other students' personal info)
    [HttpGet]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<List<StudentResponse>>> GetAll()
    {
        var students = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Route)
            .Select(s => new StudentResponse
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = s.User!.FullName,
                Email = s.User.Email,
                RollNumber = s.RollNumber,
                Batch = s.Batch,
                Department = s.Department,
                Semester = s.Semester,
                RouteId = s.RouteId,
                RouteName = s.Route != null ? s.Route.RouteName : null
            })
            .ToListAsync();

        return Ok(students);
    }

    // GET /api/students/{id} - admins only
    [HttpGet("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<StudentResponse>> GetById(Guid id)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        return Ok(new StudentResponse
        {
            Id = student.Id,
            UserId = student.UserId,
            FullName = student.User!.FullName,
            Email = student.User.Email,
            RollNumber = student.RollNumber,
            Batch = student.Batch,
            Department = student.Department,
            Semester = student.Semester,
            RouteId = student.RouteId,
            RouteName = student.Route?.RouteName
        });
    }

    // GET /api/students/me - a student can view their own record
    [HttpGet("me")]
    public async Task<ActionResult<StudentResponse>> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return NotFound(new { message = "Student profile not found for this account." });
        }

        return Ok(new StudentResponse
        {
            Id = student.Id,
            UserId = student.UserId,
            FullName = student.User!.FullName,
            Email = student.User.Email,
            RollNumber = student.RollNumber,
            Batch = student.Batch,
            Department = student.Department,
            Semester = student.Semester,
            RouteId = student.RouteId,
            RouteName = student.Route?.RouteName
        });
    }

    // PUT /api/students/{id} - admins only
    [HttpPut("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, StudentRequest request)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        if (await _db.Students.AnyAsync(s => s.RollNumber == request.RollNumber && s.Id != id))
        {
            return BadRequest(new { message = "Another student already uses this roll number." });
        }

        student.RollNumber = request.RollNumber;
        student.Batch = request.Batch;
        student.Department = request.Department;
        student.Semester = request.Semester;
        student.RouteId = request.RouteId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/students/{id} - admins only
    [HttpDelete("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}