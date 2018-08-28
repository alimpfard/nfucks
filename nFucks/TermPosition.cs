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
		public void Set(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static TermPosition operator +(TermPosition p0, TermPosition p1)
		{
			return new TermPosition(p0.X + p1.X, p0.Y + p1.Y);
		}
		public static TermPosition operator +(TermPosition p0, TermSize p1)
        {
            return new TermPosition(p0.X + p1.X, p0.Y + p1.Y);
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
		public override string ToString()
		{
			return string.Format("<{0}, {1}>", X, Y);
		}
	}
}