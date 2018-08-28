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
    }
}
