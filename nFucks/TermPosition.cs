using System;

namespace nFucks
{
	public struct TermPosition
	{
		public int X, Y;

		public TermPosition(int v1, int v2) : this()
		{
			X = v1;
			Y = v2;
		}
		public void advance(TermSize bounds)
		{
			Y++;
			if (Y >= bounds.Y)
			{
				X++;
				Y -= bounds.Y;
				if (X >= bounds.X) X -= bounds.X;
			}
		}
		public void advanceDown(TermSize bounds)
		{
			X++;
			if (X >= bounds.X)
			{
				Y++;
				X -= bounds.X;
				if (Y >= bounds.Y) Y -= bounds.Y;
			}
		}
        public void Translate(int x, int y)
        {
            X += x;
            Y += y;
        }
		public void Set(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static TermPosition operator +(TermPosition p0, TermPosition p1)
		{
			return new TermPosition(p0.X + p1.X, p0.Y + p1.Y);
		}
		public static TermPosition operator -(TermPosition p0, TermPosition p1)
        {
            return new TermPosition(p0.X - p1.X, p0.Y - p1.Y);
        }
		public static TermPosition operator +(TermPosition p0, TermSize p1)
        {
            return new TermPosition(p0.X + p1.X, p0.Y + p1.Y);
        }
		public static TermPosition operator +(TermPosition p0, int p1)
        {
            return new TermPosition(p0.X + p1, p0.Y + p1);
        }
		public TermPosition ScaleUp(int xs, int ys)
		{
			return new TermPosition(xs * X, ys * Y);
		}
		public TermPosition ScaledUp(int xs, int ys)
		{
			X *= xs;
			Y *= ys;
			return this;
		}
		public int Size
		{
			get
			{
				return (int)(System.Math.Sqrt(System.Math.Pow(X, 2) + System.Math.Pow(Y, 2)) + 0.5); // Fix thou rounding errors
			}
		}
		public override string ToString()
		{
			return string.Format("<{0}, {1}>", X, Y);
		}

		internal void ScaledDown(int xscale, int yscale)
		{
			X /= xscale;
			Y /= yscale;
		}

		internal TermPosition CompoundMaxmimumBound(TermPosition positionDelta)
		{
			return new TermPosition(Math.Min(X, X - positionDelta.X), Math.Min(Y, Y - positionDelta.Y));
		}

		internal TermPosition CompoundMaxmimumBound(TermSize positionAddition)
        {
			return new TermPosition(Math.Max(X, X + positionAddition.X), Math.Max(Y, Y + positionAddition.Y));
        }

		internal void advanceLeft(TermSize bounds)
		{
			Y--;
            if (Y < 0)
            {
                X--;
                Y += bounds.Y;
                if (X < 0) X += bounds.X;
            }
		}
		internal void advanceUp(TermSize bounds)
        {
            X--;
            if (X < 0)
            {
                Y--;
                X += bounds.X;
                if (Y < 0) Y += bounds.Y;
            }
        }
	}
}
