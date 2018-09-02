using System;

namespace nFucks {
    static class Utils {
        public static T[, ] ResizeArray<T> (ref T[, ] original, int rows, int cols) {
            var newArray = new T[rows, cols];
            int minRows = Math.Min (rows, original.GetLength (0));
            int minCols = Math.Min (cols, original.GetLength (1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            original = newArray;
            return newArray;
        }
    }

    static class IRCColor {
        public static ITermColor getITermColor (int color) {
            switch (color) {
                switch (color) {
                case  0: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.White);
                case  1: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Black);
                case  2: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkBlue);
                case  3: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkGreen);
                case  4: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Red);
                case  5: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkRed);
                case  6: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkMagenta);
                case  7: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkYellow);
                case  8: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Yellow);
                case  9: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Green);
                case 10: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkCyan);
                case 11: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Cyan);
                case 12: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Blue);
                case 13: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Magenta);
                case 14: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.DarkGray);
                case 15: 
                        return new StaticSingleTermColorProvider(System.ConsoleColor.Gray);
                default: 
                        return BasicColor.Default; // unknown color
            }
        }
    }
}
