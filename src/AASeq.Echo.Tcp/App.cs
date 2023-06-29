namespace AASeqEchoTcp;
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
            IsRequired = true,
        };
        portOption.AddValidator(portOption => {
            var port = portOption.GetValueOrDefault<int>();
            if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort) {
                portOption.ErrorMessage = $"Port must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}";
            }
        });

        var rootCommand = new RootCommand("TCP Echo") {
            ipOption,
            portOption,
        };
        rootCommand.SetHandler(
            (ip, port) => {
                Simulator.Run(ip ?? IPAddress.Any, port);
            },
            ipOption, portOption);


    }
}
