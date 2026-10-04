using Microsoft.AspNetCore.SignalR;

namespace UOGTransport.API.Hubs;

public class TrackingHub : Hub
{
    // Clients call this to "subscribe" to a specific trip's updates
    public async Task JoinTripGroup(string tripId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
    }

    public async Task LeaveTripGroup(string tripId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
    }
}