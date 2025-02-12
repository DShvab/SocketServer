namespace SocketServer.Models;

public record ClientSessionData
{
    public required string Address { get; init; }
    public int Sum { get; set; }
}