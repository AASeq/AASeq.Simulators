namespace AASeqEchoUdp;
using System;
using System.Globalization;
using System.Text;

internal static class Out {

    private static readonly object SyncRoot = new();

    public static void WriteLine(string text = "", ConsoleColor? color = null) {
        lock (SyncRoot) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff  ", CultureInfo.InvariantCulture));
            Console.ResetColor();

            if (color != null) { Console.ForegroundColor = color.Value; }
            Console.WriteLine(text);
            if (color != null) { Console.ResetColor(); }
        }
    }

    public static void WriteBytes(byte[] buffer, ConsoleColor? color = null) {
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
            Out.WriteLine(sbLine.ToString(), color);
        }
    }

}
