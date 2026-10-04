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
[Route("api/trips/{tripId}/[controller]")]
[Authorize]
public class GpsLocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GpsLocationsController(AppDbContext db)
    {
        _db = db;
    }

    private static GpsLocationResponse ToResponse(GpsLocation g) => new()
    {
        Id = g.Id,
        TripId = g.TripId,
        Latitude = g.Latitude,
        Longitude = g.Longitude,
        Speed = g.Speed,
        Heading = g.Heading,
        RecordedAt = g.RecordedAt,
        ReceivedAt = g.ReceivedAt
    };

    // POST /api/trips/{tripId}/gpslocations - only the driver who owns the active trip can send GPS
    [HttpPost]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<GpsLocationResponse>> Send(Guid tripId, GpsLocationRequest request)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
        if (driver == null)
        {
            return BadRequest(new { message = "Driver profile not found for this account." });
        }

        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        if (trip.DriverId != driver.Id)
        {
            return Forbid();
        }

        if (trip.Status != TripStatus.Active)
        {
            return BadRequest(new { message = "GPS can only be sent for an active trip." });
        }

        var location = new GpsLocation
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            Heading = request.Heading,
            RecordedAt = request.RecordedAt,
            ReceivedAt = DateTime.UtcNow
        };

        _db.GpsLocations.Add(location);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(location));
    }

    // GET /api/trips/{tripId}/gpslocations - anyone logged in can view a trip's GPS history
    [HttpGet]
    public async Task<ActionResult<List<GpsLocationResponse>>> GetAll(Guid tripId)
    {
        var locations = await _db.GpsLocations
            .Where(g => g.TripId == tripId)
            .OrderBy(g => g.RecordedAt)
            .ToListAsync();

        return Ok(locations.Select(ToResponse).ToList());
    }

    // GET /api/trips/{tripId}/gpslocations/latest - most recent point (for live tracking)
    [HttpGet("latest")]
    public async Task<ActionResult<GpsLocationResponse>> GetLatest(Guid tripId)
    {
        var location = await _db.GpsLocations
            .Where(g => g.TripId == tripId)
            .OrderByDescending(g => g.RecordedAt)
            .FirstOrDefaultAsync();

        if (location == null)
        {
            return NotFound(new { message = "No GPS data yet for this trip." });
        }

        return Ok(ToResponse(location));
    }
}