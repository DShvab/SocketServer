using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketServer.Constants;
using SocketServer.Models;

namespace SocketServer.Services;

public class ClientHandler
{
    private readonly ConcurrentDictionary<string, ClientSessionData> _clientSessions = [];

    public async Task HandleClient(TcpClient client)
    {
        string? clientAddress = (client.Client.RemoteEndPoint as IPEndPoint)?.ToString();
        if (string.IsNullOrWhiteSpace(clientAddress))
        {
            Console.WriteLine($"Invalid client address: {clientAddress}");
            return;
        }

        Console.WriteLine($"Client is connected: {clientAddress}");

        AddClientSession(clientAddress);

        await using NetworkStream stream = client.GetStream();
        try
        {
            await HandleCommands(stream, clientAddress);
        }
        finally
        {
            Console.WriteLine($"Disconnect client: {clientAddress}");
            _clientSessions.TryRemove(clientAddress, out _);
            client.Close();
        }
    }

    private void AddClientSession(string clientAddress)
    {
        _clientSessions[clientAddress] = new ClientSessionData
        {
            Address = clientAddress,
            Sum = 0
        };
    }

    private async Task HandleCommands(NetworkStream stream, string clientAddress)
    {
        using StreamReader reader = new(stream, Encoding.UTF8);
        await using StreamWriter writer = new(stream, Encoding.UTF8);
        writer.AutoFlush = true;

        await writer.WriteLineAsync("Welcome! Please enter a number or command.");

        try
        {
            while (true)
            {
                string? input = await ReadLineAsync(reader);
                if (input.IsCommand(ClientCommand.Exit))
                    break;

                if (input.IsCommand(ClientCommand.List))
                    await writer.WriteLineAsync(ClientSessionsToString());
                else if (int.TryParse(input, out int number))
                {
                    ClientSessionData session = _clientSessions[clientAddress];
                    session.Sum += number;
                    await writer.WriteLineAsync($"Current sum: {session.Sum}");
                }
                else
                    await writer.WriteLineAsync($"Invalid command. Please enter number, {ClientCommand.List} or {ClientCommand.Exit}.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Handle client exception {clientAddress}: {e.Message}");
        }
    }

    private async Task<string?> ReadLineAsync(StreamReader stream) =>
        (await stream.ReadLineAsync())?.Trim();

    private string ClientSessionsToString()
    {
        StringBuilder clientsSessionsMessage = new();

        clientsSessionsMessage.AppendLine("List of connected clients:");
        foreach (KeyValuePair<string, ClientSessionData> clientSession in _clientSessions)
            clientsSessionsMessage.AppendLine($"Address: {clientSession.Value.Address}; Sum: {clientSession.Value.Sum}.");

        return clientsSessionsMessage.ToString();
    }
}