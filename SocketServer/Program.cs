using System.Net;
using System.Net.Sockets;
using SocketServer.Services;

if (args.Length == 0 || !int.TryParse(args[0], out int port) || IsInvalidPort())
{
    Console.WriteLine("Please enter valid port (between 1024 and 65535) as argument.");
    Console.ReadLine();
    return;
}

ClientHandler clientHandler = new();

Console.WriteLine($"Launching server on port '{port}'...");
TcpListener server = new(IPAddress.Any, port);
server.Start();

Console.WriteLine("Server is started. Waiting for connections...");

try
{
    while (true)
    {
        TcpClient client = await server.AcceptTcpClientAsync();
        _ = Task.Run(() => clientHandler.HandleClient(client));
    }
}
finally
{
    server.Stop();
}

bool IsInvalidPort()
{
    const int minPort = 1024;
    const int maxPort = 65535;

    return port is < minPort or > maxPort;
}