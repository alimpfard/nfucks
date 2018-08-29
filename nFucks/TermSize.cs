using System;
namespace nFucks
{
    public class TermSize
    {
        public int X, Y;

        public TermSize(int windowHeight, int windowWidth)
        {
            X = windowHeight;
            Y = windowWidth;
        }
		public static bool operator ==(TermSize ts0, TermSize ts1)
		{
			return ts0.X == ts1.Y && ts0.Y == ts1.Y;
		}
		public static bool operator !=(TermSize ts0, TermSize ts1)
        {
            return ts0.X == ts1.Y && ts0.Y == ts1.Y;
        }
        public static TermSize CurrentTermSize => new TermSize(Console.BufferHeight, Console.BufferWidth);
        public TermSize Scaled(int x, int y)
        {
            X /= x;
            Y /= y;
            return this;
        }
		public TermSize Scale(int x, int y)
        {
			return new TermSize(X / x, Y / y);
        }

		public override bool Equals(object obj)
		{
			return obj is TermSize && this == (TermSize) obj;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("<{0}x{1}>", X, Y);
		}
	}
}
