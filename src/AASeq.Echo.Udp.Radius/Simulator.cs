namespace AASeqEchoUdpRadius;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

internal static class Simulator {

    public static void Run(IPAddress ip, int port, string secret) {
        try {
            var localEndpoint = new IPEndPoint(ip, port);
            using var udpClient = new UdpClient(localEndpoint);
            Out.WriteLine($"Listening on {localEndpoint}", ConsoleColor.Cyan);

            try {
                while (true) {
                    var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var inBuffer = udpClient.Receive(ref remoteEndpoint);

                    Out.WriteLine($"Datagram from {remoteEndpoint}", ConsoleColor.Blue);
                    Out.WriteBytes(inBuffer);

                    var outBuffer = new byte[inBuffer.Length];
                    Buffer.BlockCopy(inBuffer, 0, outBuffer, 0, inBuffer.Length);
                    outBuffer[0] = inBuffer[0] switch {
                        1 => 2,
                        4 => 5,
                        12 => 13,
                        _ => 255,
                    };
                    SetupResponseAuthenticator(ref outBuffer, secret);
                    Out.WriteBytes(outBuffer, ConsoleColor.Magenta);
                    udpClient.Send(outBuffer, outBuffer.Length, remoteEndpoint);
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

    private static void SetupResponseAuthenticator(ref byte[] buffer, string secret) {
        var secretBytes = System.Text.Encoding.UTF8.GetBytes(secret);
        using var md5 = MD5.Create();
        md5.TransformBlock(buffer, 0, buffer.Length, null, 0);
        md5.TransformFinalBlock(secretBytes, 0, secretBytes.Length);
        var md5Bytes = md5.Hash!;
        Buffer.BlockCopy(md5Bytes, 0, buffer, 4, 16);
    }

}
