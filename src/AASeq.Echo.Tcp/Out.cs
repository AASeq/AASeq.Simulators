namespace AASeqEchoTcp;
using System;
using System.Globalization;

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

}
