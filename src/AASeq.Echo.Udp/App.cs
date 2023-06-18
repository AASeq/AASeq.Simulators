namespace AASeqEchoUdp;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        var rootCommand = new RootCommand("UDP Echo") {
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
        try {
            var localEndpoint = new IPEndPoint(ip, port);
            using var udpClient = new UdpClient(localEndpoint);
            Out.WriteLine($"Listening on {localEndpoint}", ConsoleColor.Cyan);

            try {
                while (true) {
                    var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = udpClient.Receive(ref remoteEndpoint);
                    Out.WriteLine($"Datagram from {remoteEndpoint}", ConsoleColor.Blue);
                    udpClient.Send(buffer, buffer.Length, remoteEndpoint);

                    var length = buffer.Length;
                    var lineBuffer = new byte?[16];
                    for (var x = 0; x < length; x += 16) {
                        for (var j = 0; j < 16; j++) {
                            var i = x + j;
                            if (i < length) {
                                lineBuffer[j] = buffer[i];
                            } else {
                                lineBuffer[j] = null;
                            }
                        }

                        var sbLine = new StringBuilder();
                        sbLine.Append(CultureInfo.InvariantCulture, $"{x:X4} ");
                        for (var i = 0; i < 16; i++) {
                            if (i == 8) { sbLine.Append(' '); }
                            var b = lineBuffer[i];
                            if (b != null) {
                                sbLine.Append(CultureInfo.InvariantCulture, $" {b:X2}");
                            } else {
                                sbLine.Append(CultureInfo.InvariantCulture, $"   ");
                            }
                        }
                        sbLine.Append("  ");
                        for (var i = 0; i < 16; i++) {
                            if (i == 8) { sbLine.Append(' '); }
                            var b = lineBuffer[i];
                            if (b != null) {
                                sbLine.Append((b is >= 32 and <= 126) ? (char)b : '·');
                            } else {
                                sbLine.Append(CultureInfo.InvariantCulture, $" ");
                            }
                        }
                        Out.WriteLine(sbLine.ToString());
                    }
                }
            } catch (SocketException ex) {
                Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
            } finally {
                udpClient.Close();
            }
        } catch (SocketException ex) {
            Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
        }

    }
}
