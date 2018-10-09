using System;

namespace nFucks {
    public partial class FucksManager {
        public class WithBurrowedCell : IDisposable, IFucksSurface {
            private TermCell[, ] Cells;
            private TermResolution Res, pRes;
            private TermPosition[] Addrs;

            private TermCell[, ] mCells; // a <pRes.Xscale, pRes.Yscale*Addrs.length> matrix, with all the cells spread to 1x1 
            private TermSize bounds;
            private bool no_respect_old_pattern;
            public WithBurrowedCell (ref TermCell[, ] cells, ref TermResolution res, TermPosition[] cell_addrs, bool new_pat = false) {
                no_respect_old_pattern = new_pat;
                Addrs = cell_addrs;
                pRes = res;
                Res = new TermResolution (res.Xscale, Addrs.Length * res.Yscale);
                Cells = cells;
                mCells = new TermCell[Res.Xres, Res.Yres];
                for (int idx = 0; idx < Addrs.Length; idx++) {
                    ref
                    var addr = ref Addrs[idx];
                    var maddry = idx * pRes.Yscale;
                    ref
                    var cell = ref Cells[addr.X, addr.Y];
                    var fpat = cell.FillPattern;
                    for (int i = 0; i < pRes.Xscale; i++)
                        for (int j = 0; j < pRes.Yscale; j++) {
                            ref
                            var mcell = ref mCells[i, maddry + j];
                            char patc = fpat[i, j];
                            mcell.UnsafeData = patc == FucksSurfaceManager.FillValue ? cell.Data : patc;
                        }
                }
                bounds = new TermSize (Res.Xres, Res.Yres);
            }
            public void Dispose () {
                for (int idx = 0; idx < Addrs.Length; idx++) {
                    ref
                    var addr = ref Addrs[idx];
                    var maddry = idx * pRes.Yscale;
                    ref
                    var cell = ref Cells[addr.X, addr.Y];
                    var computedFillPattern = (char[, ]) cell.FillPattern.Clone (); // only the non-FillValue patterns will be changed
                    char? data = null;
                    for (int i = 0; i < computedFillPattern.GetLength (0); i++)
                        for (int j = 0; j < computedFillPattern.GetLength (1); j++) {
                            ref
                            var mcell = ref mCells[i, maddry + j];
                            var mfp = mcell.FillPattern;
                            char pat = no_respect_old_pattern && mfp != null ? mfp[0, 0] : computedFillPattern[i, j];
                            if (pat == FucksSurfaceManager.FillValue) {
                                if (data != null && mcell.Data != data) {
                                    if (no_respect_old_pattern)
                                        computedFillPattern[i, j] = mcell.Data;
                                    else
                                        throw new InvalidOperationException ($"Inconsistency in cell pattern and spread values: Expected '{data}' but found '{mcell.Data}'");
                                } else
                                    data = mcell.Data;
                            } else
                                computedFillPattern[i, j] = mcell.Data;
                        }
                    cell.FillPattern = computedFillPattern;
                    cell.Data = data ?? cell.Data;
                }
            }
            /// <summary>
            /// Puts a character into a cell complex
            /// </summary>
            /// <param name="c">Character to put.</param>
            /// <param name="position">Local (surface-wide) position of the cell.</param>
            public void PutChar (char c, TermPosition position) {
                ref
                var cell = ref mCells[position.X, position.Y];
                cell.Data = c;
            }

            /// <summary>
            /// Puts a character into a cell complex
            /// </summary>
            /// <param name="c">Character to put.</param>
            /// <param name="position">(reference) Local (surface-wide) position of the cell.</param>
            public void PutChar (char c, ref TermPosition position) {
                ref
                var cell = ref mCells[position.X, position.Y];
                cell.Data = c;
                if (c == '\n' || c == '\r')
                    position.advanceDown (bounds);
                else
                    position.advanceRight (bounds);
            }

            /// <summary>
            /// Spreads a string across the surface 
            /// </summary>
            /// <param name="s">The string.</param>
            /// <param name="pos">Starting position.</param>
            public void PutString (string s, ref TermPosition pos) {
                int strlen = s.Length;
                if (strlen == 0) return;
                int str_idx = 0;
                int c_x;
                for (c_x = pos.Y; pos.X < Res.Xres; pos.X++) {
                    for (; pos.Y < Res.Yres; pos.Y++) {
                        if (str_idx == strlen) { pos.Y--; return; }
                        PutChar (s[str_idx++], pos);
                    }
                    if (str_idx == strlen) {
                        pos.Y--;
                        return;
                    }
                    pos.Y = c_x;
                }
            }

            /// <summary>
            /// Sets the background color of a cell.
            /// </summary>
            /// <param name="position">Local position of the cell.</param>
            /// <param name="back">Color.</param>
            public void SetBackColor (TermPosition position, ITermColor back) {
                ref
                var cell = ref mCells[position.X, position.Y];
                cell.backgroundColor = back;
                cell.dirty = true;
                // currentState.cells[position.X, position.Y] = cell;
            }

            /// <summary>
            /// Sets the foreground color of a cell.
            /// </summary>
            /// <param name="position">Local position of the cell.</param>
            /// <param name="back">Color.</param>
            public void SetForeColor (TermPosition position, ITermColor back) {
                ref
                var cell = ref mCells[position.X, position.Y];
                cell.foregroundColor = back;
                cell.dirty = true;
            }


            public void PutIRCColoredString(string s, ref TermPosition pos)
            {
                int strlen = s.Length;
                if (strlen == 0) return;
                int str_idx = 0;
                int c_x;
                ITermColor
                bac = null,
                    fore = null;
                for (c_x = pos.Y; pos.X < Res.Xres; pos.X++)
                {
                    for (; pos.Y < Res.Yres; pos.Y++)
                    {
                        if (str_idx == strlen) { pos.Y--; return; }
                        char c = s[str_idx++];
                        if (c == '\n')
                        {
                            pos.X--;
                            break; // go to new line
                        }
                        if (c == '\x3')
                        { // start/end sequence for IRC color
                            int p_idx = str_idx;
                            bool saw_f = false;
                            char
                            c0 = s[str_idx++],
                                c1 = s[str_idx++];
                            if (Char.IsNumber(c0))
                            { // valid color
                                if (Char.IsNumber(c1))
                                {
                                    // double-digit fore
                                    fore = IRCColor.getITermColor(10 * (c0 - '0') + (c1 - '0'));
                                    c1 = s[str_idx++]; // get next char
                                    saw_f = true;
                                }
                                if (c1 == ',')
                                { // single-digit fore, has bac
                                    if (!saw_f)
                                        fore = IRCColor.getITermColor(c0 - '0');
                                    c = c1;
                                    int tpos = str_idx;
                                    c0 = s[str_idx++];
                                    c1 = s[str_idx++];
                                    if (Char.IsNumber(c0))
                                    { // valid color
                                        if (Char.IsNumber(c1))
                                        { // two digit color
                                            bac = IRCColor.getITermColor(10 * (c0 - '0') + (c1 - '0'));
                                            c = s[str_idx++];
                                        }
                                        else
                                        {
                                            //single-digit color
                                            bac = IRCColor.getITermColor(c0 - '0');
                                            c = c1;
                                        }
                                    }
                                    else
                                    { // invalid color; assume comma is part of the string
                                        c = c0;
                                    }
                                }
                                else
                                { // just a foreground
                                    fore = IRCColor.getITermColor(c0 - '0');
                                    c = c1;
                                }
                            }
                            else
                            { // reset color request
                                fore = bac = null;
                                c = c0;
                            }
                        }
                        if (c == '\xf')
                        { // plain
                            bac = fore = null;
                            c = s[str_idx++];
                            pos.Y--;
                            continue;
                        }
                        if (c == '\x2' || c == '\x1d' || c == '\x1f')
                        { // bold, italic, underline: ignore
                            pos.Y--;
                            continue;
                        }
                        if (c == '\x16')
                        { // reverse
                            var tmp = bac;
                            bac = fore;
                            fore = tmp;
                            pos.Y--;
                            continue;
                        }
                        PutChar(c, pos);
                        if (bac != null) SetBackColor(pos, bac);
                        if (fore != null) SetForeColor(pos, fore);
                    }
                    if (str_idx == strlen)
                    {
                        pos.Y--;
                        return;
                    }
                    pos.Y = c_x;
                }
            }
        }
    }
}