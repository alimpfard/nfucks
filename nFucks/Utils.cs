using System;
using System.Collections.Generic;
using System.Linq;

namespace nFucks {
    static public class Utils {
        public static T[, ] ResizeArray<T> (ref T[, ] original, int rows, int cols) {
            var newArray = new T[rows, cols];
            int minRows = Math.Min (rows, original.GetLength (0));
            int minCols = Math.Min (cols, original.GetLength (1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            original = newArray;
            return newArray;
        }

        static public TermPosition[] GenerateArc(int x, int y, int rad, int start, int end)
        {
            double dstart, dend, temp;
            int stopval_start = 0, stopval_end = 0;
            int
                cx = 0, cy = rad,
                df = 1 - rad, d_e = 3,
                d_se = -2 * rad + 5,
                xpcx, xmcx, xpcy, xmcy,
                ypcx, ymcx, ypcy, ymcy;
            List<TermPosition> Cells = new List<TermPosition>();

            // Radius sanity check
            if (rad < 0)
                return new TermPosition[] { };
            // Special case: point
            if (rad == 0)
                return new TermPosition[] { new TermPosition(x, y) };
            // Octant Labels
            /* Let there be peace of mind */
            //  \ 5 | 6 /
            //   \  |  /
            //  4 \ | / 7
            //     \|/
            //------.------- x
            //     /|\
            //  3 / | \ 0
            //   /  |  \
            //  / 2 | 1 \
            //      y

            // Let there be this bitmast to set whether we're drawing 
            // a given octant; e.g. 0x00111100 = octants 2-5
            byte drawoct = 0;

            // Fix angles
            start %= 360;
            end %= 360;
            // 0 <= start, end < 360. (sometimes, start > end :: arc goes back through 0)
            while (start < 0) start += 360;
            while (end < 0) end += 360;
            start %= 360;
            end %= 360;

            // where am I drawing at?
            int startoct = start / 45;
            int endoct = end / 45;
            // do-while increments
            int oct = startoct - 1;

            do
            {
                oct = (oct + 1) % 8;
                if (oct == startoct)
                {
                    // first compute stopval_start for this octant
                    dstart = (double)start;
                    temp = 0;
                    switch (oct)
                    {
                        case 0:
                        case 3:
                            temp = Math.Sin(dstart * Math.PI / 180d);
                            break;
                        case 1:
                        case 6:
                            temp = Math.Cos(dstart * Math.PI / 180d);
                            break;
                        case 2:
                        case 5:
                            temp = -Math.Cos(dstart * Math.PI / 180d);
                            break;

                        case 4:
                        case 7:
                            temp = -Math.Sin(dstart * Math.PI / 180d);
                            break;
                    }
                    temp *= rad;
                    // Round down. always.
                    // Every draw changes drawoct, and we stop right before 
                    // the last sensibe "pixel" at floor(temp)
                    // I wish computers had graph paper and it was cheap
                    stopval_start = (int)temp;

                    // should we draw at this initial oct?
                    if (oct % 2 == 1)
                        drawoct |= (byte)(1 << oct); // bit magick (TM)
                    else
                        drawoct &= (byte)(255 - (1 << oct));
                }
                if (oct == endoct)
                {
                    // and lastly, compute stopval_end for this oct
                    dend = (double)end;
                    temp = 0;
                    switch (oct)
                    {
                        case 0:
                        case 3:
                            temp = Math.Sin(dend * Math.PI / 180d);
                            break;
                        case 1:
                        case 6:
                            temp = Math.Cos(dend * Math.PI / 180d);
                            break;
                        case 2:
                        case 5:
                            temp = -Math.Cos(dend * Math.PI / 180d);
                            break;

                        case 4:
                        case 7:
                            temp = -Math.Sin(dend * Math.PI / 180d);
                            break;
                    }
                    temp *= rad;
                    stopval_end = (int)temp;
                    if (startoct == endoct)
                    {
                        // if we do decide to draw here;
                        // we've started here, ended here, and then start here again
                        // otherwise, we only draw here, so just set it not to be drawn
                        // it will be set back to be drawn at the end
                        if (start > end)
                        {
                            // if we're in the same octant and need to draw the entire circle, 
                            // we'll have to set this to render twice
                            drawoct = 255;
                        }
                        else
                            drawoct &= (byte)(255 - (1 << oct));
                    }
                    else if (oct % 2 == 1) drawoct &= (byte)(255 - (1 << oct));
                    else
                        drawoct |= (byte)(1 << oct);
                }
                else if (oct != startoct)
                {
                    // we know it's not != endoct
                    // draw this entire octant
                    drawoct |= (byte)(1 << oct);
                }
            } while (oct != endoct);

            // now we know what octants to draw and when to draw them
            // unto the raster code

            // Draw Arc
            do
            {
                ypcy = y + cy;
                ymcy = y - cy;
                if (cx > 0)
                {
                    xpcx = x + cy;
                    xmcx = x - cy;
                    // which octant?
                    if ((drawoct & 4) != 0) Cells.Add(new TermPosition(xmcx, ypcy));
                    if ((drawoct & 2) != 0) Cells.Add(new TermPosition(xpcx, ypcy));
                    if ((drawoct & 32) != 0) Cells.Add(new TermPosition(xmcx, ymcy));
                    if ((drawoct & 64) != 0) Cells.Add(new TermPosition(xpcx, ymcy));
                }
                else
                {
                    if ((drawoct & 6) != 0) Cells.Add(new TermPosition(x, ypcy));
                    if ((drawoct & 96) != 0) Cells.Add(new TermPosition(x, ymcy));
                }
                xpcy = x + cy;
                xmcy = x - cy;
                if (cx > 0 && cx != cy)
                {
                    ypcx = y + cx;
                    ymcx = y - cx;
                    if ((drawoct & 8) != 0) Cells.Add(new TermPosition(xmcy, ypcx));
                    if ((drawoct & 1) != 0) Cells.Add(new TermPosition(xpcy, ypcx));
                    if ((drawoct & 16) != 0) Cells.Add(new TermPosition(xmcy, ymcx));
                    if ((drawoct & 128) != 0) Cells.Add(new TermPosition(xpcy, ymcx));
                }
                else if (cx == 0)
                {
                    if ((drawoct & 24) != 0) Cells.Add(new TermPosition(xmcy, y));
                    if ((drawoct & 2) != 0) Cells.Add(new TermPosition(xpcy, y));
                }

                // now update the octant mask
                if (stopval_start == cx)
                {
                    // beep-boop, no, you're a switch [start and end can be in the same octant]
                    if ((drawoct & (byte)(1 << startoct)) != 0)
                        drawoct &= (byte)(255 - (1 << startoct));
                    else
                        drawoct |= (byte)(1 << startoct);
                }
                if (stopval_end == cx)
                {
                    if ((drawoct & (byte)(1 << endoct)) != 0)
                        drawoct &= (byte)(255 - (1 << endoct));
                    else
                        drawoct |= (byte)(1 << endoct);
                }

                // update "pixels"
                if (df < 0)
                {
                    df += d_e;
                    d_e += 2;
                    d_se += 4;
                }
                else
                {
                    df += d_se;
                    d_e += 2;
                    d_se += 4;
                    cy--;
                }
                cx++;
            } while (cx <= cy);
            Cells.Sort();
            return Cells.ToArray<TermPosition>();
        }

    }

    static class IRCColor {
        public static ITermColor getITermColor (int color) {
            switch (color) {
                case 1:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "0" });
                case 2:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "127" });
                case 3:
                    return new StaticSingleTermColorProvider (new string[] { "0", "147", "2" });
                case 4:
                    return new StaticSingleTermColorProvider (new string[] { "255", "0", "0" });
                case 5:
                    return new StaticSingleTermColorProvider (new string[] { "127", "0", "0" });
                case 6:
                    return new StaticSingleTermColorProvider (new string[] { "156", "0", "156" });
                case 7:
                    return new StaticSingleTermColorProvider (new string[] { "252", "127", "0" });
                case 8:
                    return new StaticSingleTermColorProvider (new string[] { "255", "255", "0" });
                case 9:
                    return new StaticSingleTermColorProvider (new string[] { "0", "252", "0" });
                case 10:
                    return new StaticSingleTermColorProvider (new string[] { "0", "147", "147" });
                case 11:
                    return new StaticSingleTermColorProvider (new string[] { "0", "255", "255" });
                case 12:
                    return new StaticSingleTermColorProvider (new string[] { "0", "0", "252" });
                case 13:
                    return new StaticSingleTermColorProvider (new string[] { "255", "0", "255" });
                case 14:
                    return new StaticSingleTermColorProvider (new string[] { "127", "127", "127" });
                case 15:
                    return new StaticSingleTermColorProvider (new string[] { "210", "210", "210" });
                case 0:
                    return new StaticSingleTermColorProvider (new string[] { "255", "255", "255" });
                default:
                    return BasicColor.Default; // unknown color
            }
        }
    }
}