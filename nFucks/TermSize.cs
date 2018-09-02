using System;
namespace nFucks {
    public class TermSize {
        public int X, Y;

        public TermSize (int windowHeight, int windowWidth) {
            X = windowHeight;
            Y = windowWidth;
        }
        public static bool operator == (TermSize ts0, TermSize ts1) {
            return ts0.X == ts1.Y && ts0.Y == ts1.Y;
        }
        public static bool operator != (TermSize ts0, TermSize ts1) {
            return ts0.X != ts1.Y && ts0.Y != ts1.Y;
        }
        public static TermSize CurrentTermSize => new TermSize (Console.WindowHeight, Console.WindowWidth);
        public static TermSize TrySetTerminalSize (int x, int y) {
            Console.SetWindowSize(x, y);
            var ts = CurrentTermSize;
            if (ts.X != x || ts.Y != y)
                throw new InvalidOperationException("Could not change terminal size");
            return CurrentTermSize;
        }
        public TermSize Scaled (int x, int y) {
            x = x == 0 ? 1 : x;
            y = y == 0 ? 1 : y;
            X /= x;
            Y /= y;
            return this;
        }
        public TermSize Scale (int x, int y) {
            x = x == 0 ? 1 : x;
            y = y == 0 ? 1 : y;
            return new TermSize (X / x, Y / y);
        }
        public override bool Equals (object obj) {
            return obj is TermSize && this == (TermSize) obj;
        }

        public override int GetHashCode () {
            return base.GetHashCode ();
        }

        public override string ToString () {
            return String.Format ("<{0}x{1}>", X, Y);
        }

        internal void ScaleUp (int xscaled, int yscaled) {
            X *= xscaled;
            Y *= yscaled;
        }
        internal TermPosition CompoundMaximumBound (int x, int y) {
            return new TermPosition (Math.Max (X, X + x), Math.Max (Y, Y + y));
        }
    }
}