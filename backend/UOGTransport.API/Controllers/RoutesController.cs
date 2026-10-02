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
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoutesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/routes - any logged-in user can view
    [HttpGet]
    public async Task<ActionResult<List<RouteResponse>>> GetAll()
    {
        var routes = await _db.Routes
            .Select(r => new RouteResponse
            {
                Id = r.Id,
                RouteName = r.RouteName,
                StartPoint = r.StartPoint,
                EndPoint = r.EndPoint,
                IsActive = r.IsActive
            })
            .ToListAsync();

        return Ok(routes);
    }

    // GET /api/routes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<RouteResponse>> GetById(Guid id)
    {
        var route = await _db.Routes.FindAsync(id);

        if (route == null)
        {
            return NotFound(new { message = "Route not found." });
        }

        return Ok(new RouteResponse
        {
            Id = route.Id,
            RouteName = route.RouteName,
            StartPoint = route.StartPoint,
            EndPoint = route.EndPoint,
            IsActive = route.IsActive
        });
    }

    // POST /api/routes - only admins
    [HttpPost]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<ActionResult<RouteResponse>> Create(RouteRequest request)
    {
        var route = new BusRoute
        {
            Id = Guid.NewGuid(),
            RouteName = request.RouteName,
            StartPoint = request.StartPoint,
            EndPoint = request.EndPoint,
            IsActive = request.IsActive
        };

        _db.Routes.Add(route);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = route.Id }, new RouteResponse
        {
            Id = route.Id,
            RouteName = route.RouteName,
            StartPoint = route.StartPoint,
            EndPoint = route.EndPoint,
            IsActive = route.IsActive
        });
    }

    // PUT /api/routes/{id} - only admins
    [HttpPut("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, RouteRequest request)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound(new { message = "Route not found." });
        }

        route.RouteName = request.RouteName;
        route.StartPoint = request.StartPoint;
        route.EndPoint = request.EndPoint;
        route.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/routes/{id} - only admins
    [HttpDelete("{id}")]
    [Authorize(Roles = "TransportAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound(new { message = "Route not found." });
        }

        // Prevent deleting a route that still has buses assigned
        var hasBuses = await _db.Buses.AnyAsync(b => b.RouteId == id);
        if (hasBuses)
        {
            return BadRequest(new { message = "Cannot delete a route that has buses assigned to it." });
        }

        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}