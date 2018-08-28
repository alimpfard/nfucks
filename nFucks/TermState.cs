using System;
namespace nFucks
{
    public struct TermState
    {
		public TermResolution resolution;
		public TermCell[,] cells;
		public TermPosition cursor;
		public TermState(TermResolution res)
		{
			resolution = res;
			cells = new TermCell[res.Xres,res.Yres];
			cursor = new TermPosition(0, 0);
		}
    }
}
