namespace AASeqEchoUdpRadius;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

internal static class App {

    internal static void Main(string[] args) {
        var ipOption = new Option<IPAddress?>(
            aliases: new[] { "--ip" },
            description: "IP to listen on") {
            Arity = ArgumentArity.ExactlyOne,
        };

        var portOption = new Option<int?>(
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

        var secretOption = new Option<string?>(
            aliases: new[] { "--secret", "-s" },
            description: "Secret to use") {
            Arity = ArgumentArity.ExactlyOne,
        };

        var rootCommand = new RootCommand("UDP RADIUS Echo") {
            ipOption,
            portOption,
            secretOption,
        };
        rootCommand.SetHandler(
            (ip, port, secret) => {
                Simulator.Run(ip ?? IPAddress.Any, port ?? 1812, secret ?? "secret");
            },
            ipOption, portOption, secretOption);

        rootCommand.Invoke(args);
    }

}
