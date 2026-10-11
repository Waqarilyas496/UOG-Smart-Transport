using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;
using UOGTransport.API.Enums;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly AppDbContext _db;

    public DriversController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/drivers/me - the logged-in driver's own profile + assigned bus/route
    [HttpGet("me")]
    [Authorize(Roles = "Driver")]
    public async Task<ActionResult<DriverMeResponse>> GetMe()
    {
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var driver = await _db.Drivers
            .Include(d => d.User)
            .Include(d => d.AssignedBus).ThenInclude(b => b!.Route)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
        {
            return NotFound(new { message = "Driver profile not found for this account." });
        }

        var activeTrip = await _db.Trips
            .Where(t => t.DriverId == driver.Id && t.Status == TripStatus.Active)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();

        return Ok(new DriverMeResponse
        {
            DriverId = driver.Id,
            FullName = driver.User!.FullName,
            LicenseNumber = driver.LicenseNumber,
            BusId = driver.AssignedBusId,
            BusNumber = driver.AssignedBus?.BusNumber,
            RouteId = driver.AssignedBus?.RouteId,
            RouteName = driver.AssignedBus?.Route?.RouteName,
            ActiveTripId = activeTrip
        });
    }

    // GET /api/drivers - any logged-in user can view
    [HttpGet]
    public async Task<ActionResult<List<DriverResponse>>> GetAll()
    {
        var drivers = await _db.Drivers
            .Include(d => d.User)
            .Include(d => d.AssignedBus)
            .Select(d => new DriverResponse
            {
                Id = d.Id,
                UserId = d.UserId,
                FullName = d.User!.FullName,
                Email = d.User.Email,
                LicenseNumber = d.LicenseNumber,
                AssignedBusId = d.AssignedBusId,
                AssignedBusNumber = d.AssignedBus != null ? d.AssignedBus.BusNumber : null
            })
            .ToListAsync();

        return Ok(drivers);
    }

    // GET /api/drivers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DriverResponse>> GetById(Guid id)
    {
        var driver = await _db.Drivers
            .Include(d => d.User)
            .Include(d => d.AssignedBus)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (driver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        return Ok(new DriverResponse
        {
            Id = driver.Id,
            UserId = driver.UserId,
            FullName = driver.User!.FullName,
            Email = driver.User.Email,
            LicenseNumber = driver.LicenseNumber,
            AssignedBusId = driver.AssignedBusId,
            AssignedBusNumber = driver.AssignedBus?.BusNumber
        });
    }

    // PUT /api/drivers/{id} - only admins can update license/bus assignment
    [HttpPut("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, DriverRequest request)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        if (await _db.Drivers.AnyAsync(d => d.LicenseNumber == request.LicenseNumber && d.Id != id))
        {
            return BadRequest(new { message = "Another driver already uses this license number." });
        }

        // If assigning a bus, make sure it isn't already assigned to a different driver
        if (request.AssignedBusId != null)
        {
            var busTaken = await _db.Drivers.AnyAsync(d =>
                d.AssignedBusId == request.AssignedBusId && d.Id != id);

            if (busTaken)
            {
                return BadRequest(new { message = "This bus is already assigned to another driver." });
            }
        }

        driver.LicenseNumber = request.LicenseNumber;
        driver.AssignedBusId = request.AssignedBusId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/drivers/{id} - only admins (removes driver profile, not the User account)
    [HttpDelete("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver == null)
        {
            return NotFound(new { message = "Driver not found." });
        }

        _db.Drivers.Remove(driver);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}