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
                case 1:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "0" });
                case 2:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "127" });
                case 3:
                    return new StaticSingleTermColorProvider (new string[] { "0", "147", "2" });
                case 4:
                    return new StaticSingleTermColorProvider (new string[] { "255", "0", "0" });
                case 5:
                    return new StaticSingleTermColorProvider (new string[] { "127", "0", "0" });
                case 6:
                    return new StaticSingleTermColorProvider (new string[] { "156", "0", "156" });
                case 7:
                    return new StaticSingleTermColorProvider (new string[] { "252", "127", "0" });
                case 8:
                    return new StaticSingleTermColorProvider (new string[] { "255", "255", "0" });
                case 9:
                    return new StaticSingleTermColorProvider (new string[] { "0", "252", "0" });
                case 10:
                    return new StaticSingleTermColorProvider (new string[] { "0", "147", "147" });
                case 11:
                    return new StaticSingleTermColorProvider (new string[] { "0", "255", "255" });
                case 12:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "252" });
                case 13:
                    return new StaticSingleTermColorProvider (new string[] { "255", "0", "255" });
                case 14:
                    return new StaticSingleTermColorProvider (new string[] { "127", "127", "127" });
                case 15:
                    return new StaticSingleTermColorProvider (new string[] { "210", "210", "210" });
                case 0:
                    return new StaticSingleTermColorProvider (new string[] { "255", "255", "255" });
                default:
                    return BasicColor.Default; // unknown color
            }
        }
    }
}