using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UOGTransport.API.Data;
using UOGTransport.API.DTOs;
using UOGTransport.API.Models;

namespace UOGTransport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BusesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/buses - any logged-in user can view
    [HttpGet]
    public async Task<ActionResult<List<BusResponse>>> GetAll()
    {
        var buses = await _db.Buses
            .Include(b => b.Route)
            .Select(b => new BusResponse
            {
                Id = b.Id,
                BusNumber = b.BusNumber,
                Capacity = b.Capacity,
                Status = b.Status.ToString(),
                RouteId = b.RouteId,
                RouteName = b.Route != null ? b.Route.RouteName : null
            })
            .ToListAsync();

        return Ok(buses);
    }

    // GET /api/buses/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<BusResponse>> GetById(Guid id)
    {
        var bus = await _db.Buses.Include(b => b.Route).FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound(new { message = "Bus not found." });
        }

        return Ok(new BusResponse
        {
            Id = bus.Id,
            BusNumber = bus.BusNumber,
            Capacity = bus.Capacity,
            Status = bus.Status.ToString(),
            RouteId = bus.RouteId,
            RouteName = bus.Route?.RouteName
        });
    }

    // POST /api/buses - only admins can create
    [HttpPost]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<BusResponse>> Create(BusRequest request)
    {
        if (await _db.Buses.AnyAsync(b => b.BusNumber == request.BusNumber))
        {
            return BadRequest(new { message = "A bus with this number already exists." });
        }

        var bus = new Bus
        {
            Id = Guid.NewGuid(),
            BusNumber = request.BusNumber,
            Capacity = request.Capacity,
            Status = request.Status,
            RouteId = request.RouteId
        };

        _db.Buses.Add(bus);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = bus.Id }, new BusResponse
        {
            Id = bus.Id,
            BusNumber = bus.BusNumber,
            Capacity = bus.Capacity,
            Status = bus.Status.ToString(),
            RouteId = bus.RouteId
        });
    }

    // PUT /api/buses/{id} - only admins can update
    [HttpPut("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, BusRequest request)
    {
        var bus = await _db.Buses.FindAsync(id);
        if (bus == null)
        {
            return NotFound(new { message = "Bus not found." });
        }

        if (await _db.Buses.AnyAsync(b => b.BusNumber == request.BusNumber && b.Id != id))
        {
            return BadRequest(new { message = "Another bus already uses this number." });
        }

        bus.BusNumber = request.BusNumber;
        bus.Capacity = request.Capacity;
        bus.Status = request.Status;
        bus.RouteId = request.RouteId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/buses/{id} - only admins can delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var bus = await _db.Buses.FindAsync(id);
        if (bus == null)
        {
            return NotFound(new { message = "Bus not found." });
        }

        _db.Buses.Remove(bus);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}