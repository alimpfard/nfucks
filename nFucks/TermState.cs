using System;
namespace nFucks {
    public struct TermState {
        public TermResolution resolution;
        public TermCell[, ] cells;
        public TermState (TermResolution res) {
            resolution = res;
            cells = new TermCell[res.Xres, res.Yres];
        }
    }
}