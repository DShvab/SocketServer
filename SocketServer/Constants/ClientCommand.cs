namespace SocketServer.Constants;

public static class ClientCommand
{
    public const string List = "list";
    public const string Exit = "exit";

    public static bool IsCommand(this string? input, string command) =>
        string.Equals(input, command, StringComparison.InvariantCultureIgnoreCase);
}