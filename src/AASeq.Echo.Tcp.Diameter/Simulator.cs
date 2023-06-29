namespace AASeqEchoTcpDiameter;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal static class Simulator {

    public static void Run(IPAddress ip, int port) {
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

                        HandleClient(tcpClient);
                    } catch (SocketException ex) {
                        Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                    }
                }
            } catch (SocketException ex) {
                Out.WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
            }
        }

    }

    private static void HandleClient(TcpClient tcpClient) {
        Task.Run(() => {
            Out.WriteLine($"Connected from {tcpClient.Client.RemoteEndPoint}", ConsoleColor.Blue);

            var buffer = new byte[1024];
            try {
                var stream = tcpClient.GetStream();
                int length;
                long offset = 0;
                long nextPacketOffset = 0;

                while ((length = stream.Read(buffer, 0, buffer.Length)) > 0) {
                    var endOffset = offset + length;

                    if ((nextPacketOffset >= offset) && (nextPacketOffset < endOffset)) {
                        if (buffer[nextPacketOffset - offset] == 0x01) {
                            var diamLength = (buffer[nextPacketOffset - offset + 1] << 16) | (buffer[nextPacketOffset - offset + 2] << 8) | (buffer[nextPacketOffset - offset + 3]);
                            buffer[nextPacketOffset - offset + 4] = (byte)(buffer[nextPacketOffset - offset + 4] & 0x7F);  // remove request bit
                            nextPacketOffset = offset + diamLength;
                        } else {
                            throw new InvalidOperationException("Unexpected Diameter version at offset {nextPacketOffset}");
                        }
                    }

                    stream.Write(buffer, 0, length);

                    var lineBuffer = new byte?[16];
                    for (var startOffset = offset / 16 * 16; startOffset < endOffset; startOffset += 16) {
                        var x = startOffset - offset;
                        for (var j = 0; j < 16; j++) {
                            var i = x + j;
                            if ((i >= 0) && (i < length)) {
                                lineBuffer[j] = buffer[i];
                            } else {
                                lineBuffer[j] = null;
                            }
                        }
                        x += 16;

                        var sbLine = new StringBuilder();
                        sbLine.Append(CultureInfo.InvariantCulture, $"{startOffset:X4} ");
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

                    offset += length;
                }
            } catch (SocketException ex) {
                Out.WriteLine($"Error ({tcpClient.Client.RemoteEndPoint}): {ex.Message}", ConsoleColor.Red);
            } finally {
                Out.WriteLine($"Disconnected from {tcpClient.Client.RemoteEndPoint}", ConsoleColor.Blue);
                tcpClient.Close();
            }
        });
    }

}
