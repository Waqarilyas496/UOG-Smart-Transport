using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;
using UOGTransport.API.Enums;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/students/me/live")]
[Authorize(Roles = "Student")]
public class StudentLiveController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentLiveController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/students/me/live - where is the bus running on MY route right now?
    [HttpGet]
    public async Task<ActionResult<LiveBusResponse>> Get()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await _db.Students
            .Include(s => s.Route)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return NotFound(new { message = "Student profile not found for this account." });
        }

        if (student.RouteId == null)
        {
            return Ok(new LiveBusResponse { HasRoute = false });
        }

        var response = new LiveBusResponse
        {
            HasRoute = true,
            RouteName = student.Route?.RouteName
        };

        var trip = await _db.Trips
            .Include(t => t.Bus)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Where(t => t.RouteId == student.RouteId && t.Status == TripStatus.Active)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefaultAsync();

        if (trip == null)
        {
            return Ok(response);
        }

        response.HasActiveTrip = true;
        response.TripId = trip.Id;
        response.BusNumber = trip.Bus?.BusNumber;
        response.DriverName = trip.Driver?.User?.FullName;

        var location = await _db.GpsLocations
            .Where(g => g.TripId == trip.Id)
            .OrderByDescending(g => g.RecordedAt)
            .FirstOrDefaultAsync();

        if (location != null)
        {
            response.Latitude = location.Latitude;
            response.Longitude = location.Longitude;
            response.Speed = location.Speed;
            response.RecordedAt = location.RecordedAt;
        }

        return Ok(response);
    }
}