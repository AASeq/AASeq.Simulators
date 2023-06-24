namespace AASeqEchoTcpDiameter;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Net.Sockets;

internal static class App {

    internal static void Main(string[] args) {
        var ipOption = new Option<IPAddress?>(
            aliases: new[] { "--ip" },
            description: "IP to listen on") {
            Arity = ArgumentArity.ExactlyOne,
        };

        var portOption = new Option<int>(
            aliases: new[] { "--port", "-p" },
            description: "Port to listen on") {
            Arity = ArgumentArity.ExactlyOne,
        };
        portOption.AddValidator(portOption => {
            var port = portOption.GetValueOrDefault<int>();
            if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort) {
                portOption.ErrorMessage = $"Port must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}";
            }
        });

        var rootCommand = new RootCommand("TCP Diameter Echo") {
            ipOption,
            portOption,
        };
        rootCommand.SetHandler(
            (ip, port) => {
                Run(ip ?? IPAddress.Any, port);
            },
            ipOption, portOption);

        rootCommand.Invoke(args);
    }


    private static void Run(IPAddress ip, int port) {
        while (true) {
            try {
                if (port == 0) { port = 3868; }
                var server = new TcpListener(ip, port);
                server.Start();
                Out.WriteLine($"Listening on {server.LocalEndpoint}", ConsoleColor.Cyan);

                while (true) {
                    try {
                        var tcpClient = server.AcceptTcpClient();
                        Out.WriteLine($"Accepted connection from {tcpClient.Client.RemoteEndPoint}", ConsoleColor.DarkCyan);

                        ClientThread.Run(tcpClient);
                    } catch (SocketException ex) {
                        Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                    }
                }
            } catch (SocketException ex) {
                Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
            }
        }

    }
}
