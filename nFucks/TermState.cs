using System;
namespace nFucks {
	public struct TermState {
		public TermResolution resolution;
		public TermCell [,] cells;
		public TermState (TermResolution res)
		{
			resolution = res;
			cells = new TermCell [res.Xres, res.Yres];
		}
		public void Scale (int x, int y)
		{
			resolution.Xscale *= x;
			resolution.Yscale *= y;
			resolution.Xres--;
			resolution.Yres--;
			Utils.ResizeArray (ref cells, resolution.Xres*x, resolution.Yres*y);
		}
	}
}
