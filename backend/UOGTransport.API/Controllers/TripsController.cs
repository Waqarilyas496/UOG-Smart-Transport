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
public class TripsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TripsController(AppDbContext db)
    {
        _db = db;
    }

    // Helper: get the Driver record for the currently logged-in user
    private async Task<Driver?> GetCurrentDriverAsync()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await _db.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
    }

    private static TripResponse ToResponse(Trip t) => new()
    {
        Id = t.Id,
        BusId = t.BusId,
        BusNumber = t.Bus?.BusNumber ?? string.Empty,
        DriverId = t.DriverId,
        DriverName = t.Driver?.User?.FullName ?? string.Empty,
        RouteId = t.RouteId,
        RouteName = t.Route?.RouteName ?? string.Empty,
        StartTime = t.StartTime,
        EndTime = t.EndTime,
        Status = t.Status.ToString()
    };

    // GET /api/trips - any logged-in user can view all trips
    [HttpGet]
    public async Task<ActionResult<List<TripResponse>>> GetAll()
    {
        var trips = await _db.Trips
            .Include(t => t.Bus)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Route)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return Ok(trips.Select(ToResponse).ToList());
    }

    // GET /api/trips/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TripResponse>> GetById(Guid id)
    {
        var trip = await _db.Trips
            .Include(t => t.Bus)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Route)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        return Ok(ToResponse(trip));
    }

    // POST /api/trips/start - only Drivers can start their own trip
    [HttpPost("start")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<TripResponse>> StartTrip(StartTripRequest request)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return BadRequest(new { message = "Driver profile not found for this account." });
        }

        // Prevent starting a new trip while another is still active for this driver
        var hasActiveTrip = await _db.Trips.AnyAsync(t =>
            t.DriverId == driver.Id && t.Status == TripStatus.Active);

        if (hasActiveTrip)
        {
            return BadRequest(new { message = "You already have an active trip. End it before starting a new one." });
        }

        var busExists = await _db.Buses.AnyAsync(b => b.Id == request.BusId);
        if (!busExists)
        {
            return BadRequest(new { message = "Bus not found." });
        }

        var routeExists = await _db.Routes.AnyAsync(r => r.Id == request.RouteId);
        if (!routeExists)
        {
            return BadRequest(new { message = "Route not found." });
        }

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            BusId = request.BusId,
            DriverId = driver.Id,
            RouteId = request.RouteId,
            StartTime = DateTime.UtcNow,
            Status = TripStatus.Active
        };

        _db.Trips.Add(trip);
        await _db.SaveChangesAsync();

        // Reload with related data for the response
        await _db.Entry(trip).Reference(t => t.Bus).LoadAsync();
        await _db.Entry(trip).Reference(t => t.Driver).LoadAsync();
        await _db.Entry(trip.Driver!).Reference(d => d.User).LoadAsync();
        await _db.Entry(trip).Reference(t => t.Route).LoadAsync();

        return Ok(ToResponse(trip));
    }

    // POST /api/trips/{id}/end - only the driver who owns the trip can end it
    [HttpPost("{id}/end")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> EndTrip(Guid id)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return BadRequest(new { message = "Driver profile not found for this account." });
        }

        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == id);
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
            return BadRequest(new { message = "This trip is not currently active." });
        }

        trip.Status = TripStatus.Completed;
        trip.EndTime = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}