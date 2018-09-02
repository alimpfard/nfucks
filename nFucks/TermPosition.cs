using System;

namespace nFucks {
    public struct TermPosition {
        public int X, Y;

        public static TermPosition Origin { get => new TermPosition (0, 0); }
        public TermPosition (int v1, int v2) : this () {
            X = v1;
            Y = v2;
        }
        /// <summary>
        /// Moves the position one character to the right,
        /// wraps lines if necessary
        /// </summary>
        /// <param name="bounds">the bounds in which to move</param>
        public void advanceRight (TermSize bounds) {
            Y++;
            if (Y >= bounds.Y) {
                X++;
                Y -= bounds.Y;
                if (X >= bounds.X) X -= bounds.X;
            }
        }
        /// <summary>
        /// Moves the position one character down,
        /// wraps lines if necessary
        /// </summary>
        /// <param name="bounds">the bounds in which to move</param>
        public void advanceDown (TermSize bounds) {
            X++;
            if (X >= bounds.X) {
                Y++;
                X -= bounds.X;
                if (Y >= bounds.Y) Y -= bounds.Y;
            }
        }
        /// <summary>
        /// Moves the position one character to the left,
        /// wraps lines if necessary
        /// </summary>
        /// <param name="bounds">the bounds in which to move</param>
        internal void advanceLeft (TermSize bounds) {
            Y--;
            if (Y < 0) {
                X--;
                Y += bounds.Y;
                if (X < 0) X += bounds.X;
            }
        }
        /// <summary>
        /// Moves the position one character up,
        /// wraps lines if necessary
        /// </summary>
        /// <param name="bounds">the bounds in which to move</param>
        internal void advanceUp (TermSize bounds) {
            X--;
            if (X < 0) {
                Y--;
                X += bounds.X;
                if (Y < 0) Y += bounds.Y;
            }
        }
        /// <summary>
        /// Translates the position without bounds checking
        /// </summary>
        /// <param name="x">delta in X axis</param>
        /// <param name="y">delta in Y axis</param>
        public void Translate (int x, int y) {
            X += x;
            Y += y;
        }
        /// <summary>
        /// The absolute distance from origin
        /// </summary>
        public int Size {
            get {
                return (int) (System.Math.Sqrt (System.Math.Pow (X, 2) + System.Math.Pow (Y, 2)) + 0.5); // Fix thou rounding errors
            }
        }
        public override string ToString () {
            return string.Format ("<{0}, {1}>", X, Y);
        }

        internal void ScaledDown (int xscale, int yscale) {
            xscale = xscale == 0 ? 1 : xscale;
            yscale = yscale == 0 ? 1 : yscale;
            X /= xscale;
            Y /= yscale;
        }

        internal TermPosition CompoundMaxmimumBound (TermPosition positionDelta) {
            return new TermPosition (Math.Min (X, X - positionDelta.X), Math.Min (Y, Y - positionDelta.Y));
        }
        internal TermPosition CompoundMaxmimumBound (int x, int y) {
            return new TermPosition (Math.Max (X, X + x), Math.Max (Y, Y + y));
        }
        internal TermPosition CompoundMinimumBound (int x, int y) {
            return new TermPosition (Math.Min (X, X + x), Math.Min (Y, Y + y));
        }
        internal TermPosition CompoundMaxmimumBound (TermSize positionAddition) {
            return new TermPosition (Math.Max (X, X + positionAddition.X), Math.Max (Y, Y + positionAddition.Y));
        }
        public void Set (int x, int y) {
            X = x;
            Y = y;
        }
        public static TermPosition operator + (TermPosition p0, TermPosition p1) {
            return new TermPosition (p0.X + p1.X, p0.Y + p1.Y);
        }
        public static TermPosition operator - (TermPosition p0, TermPosition p1) {
            return new TermPosition (p0.X - p1.X, p0.Y - p1.Y);
        }
        public static TermPosition operator + (TermPosition p0, TermSize p1) {
            return new TermPosition (p0.X + p1.X, p0.Y + p1.Y);
        }
        public static TermPosition operator + (TermPosition p0, int p1) {
            return new TermPosition (p0.X + p1, p0.Y + p1);
        }
        public TermPosition ScaleUp (int xs, int ys) {
            return new TermPosition (xs * X, ys * Y);
        }
        public TermPosition ScaledUp (int xs, int ys) {
            X *= xs;
            Y *= ys;
            return this;
        }
    }
}