using System;
namespace nFucks {
    public struct TermResolution {
        public readonly int Xres, Yres, Xscale, Yscale;
        public TermResolution (int xres, int yres) {
            Xres = xres;
            Yres = yres;
            Xscale = 1;
            Yscale = 1;
        }
        public TermResolution (int xsize, int ysize, int xscale, int yscale) {
            if (xscale < 1 || yscale<1) throw new InvalidOperationException ("Scale must be>=1");
            Xscale = xscale;
            Yscale = yscale;
            Xres = xsize / xscale;
            Yres = ysize / yscale;
            if (Xres < 1 || Yres < 1) throw new InvalidOperationException ("Scale too big");
        }
    }
}