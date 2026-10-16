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
[Route("api/trips/{tripId}/passengers")]
[Authorize]
public class PassengersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PassengersController(AppDbContext db)
    {
        _db = db;
    }

    // Students whose most recent record on this trip is "Boarding" are currently on the bus
    private async Task<int> CountOnBoardAsync(Guid tripId)
    {
        var records = await _db.PassengerRecords
            .Where(r => r.TripId == tripId)
            .Select(r => new { r.StudentId, r.Type, r.ScannedAt })
            .ToListAsync();

        return records
            .GroupBy(r => r.StudentId)
            .Count(g => g.OrderByDescending(r => r.ScannedAt).First().Type == PassengerEventType.Boarding);
    }

    // POST /api/trips/{tripId}/passengers/scan - the trip's driver scans a student's pass
    [HttpPost("scan")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<ScanResponse>> Scan(Guid tripId, ScanRequest request)
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

        var trip = await _db.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == tripId);
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
            return BadRequest(new { message = "Passengers can only be scanned on an active trip." });
        }

        var passNumber = request.PassNumber.Trim();
        var pass = await _db.TransportPasses
            .Include(p => p.Student).ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(p => p.PassNumber == passNumber);

        if (pass == null)
        {
            return BadRequest(new { message = "Invalid pass." });
        }

        var now = DateTime.UtcNow;
        if (pass.Status != PassStatus.Active)
        {
            return BadRequest(new { message = "This pass is suspended or expired." });
        }

        if (now < pass.ValidFrom || now > pass.ValidUntil)
        {
            return BadRequest(new { message = "This pass is not valid today." });
        }

        if (pass.RouteId != trip.RouteId)
        {
            return BadRequest(new { message = "This pass is not valid for this route." });
        }

        // Duplicate-scan prevention: a student must alternate Boarding -> Alighting -> Boarding
        var last = await _db.PassengerRecords
            .Where(r => r.TripId == tripId && r.StudentId == pass.StudentId)
            .OrderByDescending(r => r.ScannedAt)
            .FirstOrDefaultAsync();

        if (last != null && last.Type == request.Type)
        {
            var msg = request.Type == PassengerEventType.Boarding
                ? "This student is already on the bus."
                : "This student has already left the bus.";
            return BadRequest(new { message = msg });
        }

        if (last == null && request.Type == PassengerEventType.Alighting)
        {
            return BadRequest(new { message = "This student has not boarded on this trip." });
        }

        var record = new PassengerRecord
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            StudentId = pass.StudentId,
            Type = request.Type,
            ScannedAt = now,
            ScannedByDriverId = driver.Id
        };

        _db.PassengerRecords.Add(record);
        await _db.SaveChangesAsync();

        return Ok(new ScanResponse
        {
            StudentName = pass.Student?.User?.FullName ?? string.Empty,
            RollNumber = pass.Student?.RollNumber ?? string.Empty,
            Type = request.Type.ToString(),
            ScannedAt = now,
            OnBoard = await CountOnBoardAsync(tripId),
            Capacity = trip.Bus?.Capacity ?? 0
        });
    }

    // GET /api/trips/{tripId}/passengers/occupancy - anyone logged in
    [HttpGet("occupancy")]
    public async Task<ActionResult<OccupancyResponse>> GetOccupancy(Guid tripId)
    {
        var trip = await _db.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip == null)
        {
            return NotFound(new { message = "Trip not found." });
        }

        return Ok(new OccupancyResponse
        {
            TripId = tripId,
            OnBoard = await CountOnBoardAsync(tripId),
            Capacity = trip.Bus?.Capacity ?? 0
        });
    }
}