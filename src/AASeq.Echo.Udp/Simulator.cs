namespace AASeqEchoUdp;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

internal static class Simulator {

    public static void Run(IPAddress ip, int port) {
        try {
            var localEndpoint = new IPEndPoint(ip, port);
            using var udpClient = new UdpClient(localEndpoint);
            Out.WriteLine($"Listening on {localEndpoint}", ConsoleColor.Cyan);

            try {
                while (true) {
                    var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = udpClient.Receive(ref remoteEndpoint);
                    Out.WriteLine($"Datagram from {remoteEndpoint}", ConsoleColor.Blue);
                    Out.WriteBytes(buffer);

                    Out.WriteBytes(buffer, ConsoleColor.Magenta);
                    udpClient.Send(buffer, buffer.Length, remoteEndpoint);
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
