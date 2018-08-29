using System;
using System.Collections.Generic;
using System.Linq;

namespace nFucks {
    public class FucksManager {
        List<FucksSurfaceManager> surfaces = new List<FucksSurfaceManager> ();
        private bool invalidated = false;
        private bool dirty = false;
        private GlobalTermInfo currentInfo = new GlobalTermInfo (TermSize.CurrentTermSize);
        private GlobalTermState[, ] renderState;
        private Natives natives = new Natives ();
        public FucksManager () {
            natives.SetConsoleRaw ();
            renderState = new GlobalTermState[currentInfo.size.X, currentInfo.size.Y];
        }
        ~FucksManager () {
            natives.RestoreConsoleMode ();
            Console.ResetColor();
        }
        /// <summary>
        /// Creates a surface without initializing it
        /// </summary>
        /// <param name="position">the top-left corner of the surface, in global terms</param>
        /// <param name="resolution">the resolution of the surface</param>
        /// <returns>The surface</returns>
        public FucksSurfaceManager CreateSurface (TermPosition position, TermResolution resolution) {
            var sz = new TermSize (resolution.Xres, resolution.Yres);
            var manager = new FucksSurfaceManager (position, sz, resolution, BasicColor.Default);
            surfaces.Add (manager);
            return manager;
        }
        /// <summary>
        /// Creates and initializes a surface
        /// </summary>
        /// <param name="position">the top-left corner of the surface, in global terms</param>
        /// <param name="resolution">the resolution of the surface</param>
        /// <returns></returns>
        public FucksSurfaceManager CreateAndInitializeSurface (TermPosition position, TermResolution resolution) {
            var sz = new TermSize (resolution.Xres, resolution.Yres);
            var manager = new FucksSurfaceManager (position, sz, resolution, BasicColor.Default);
            manager.Initialize ();
            surfaces.Add (manager);
            return manager;
        }

        public void PutChar(FucksSurfaceManager surface, char c, ref TermPosition termPosition)
        {
            surface.PutChar(c, ref termPosition);
            dirty = surface.Dirty;
        }

        public void PutChar(FucksSurfaceManager surface, char c, TermPosition termPosition)
        {
            surface.PutChar(c, ref termPosition);
            dirty = surface.Dirty;
        }
        

        /// <summary>
        /// Brings a surface to the top, 
        /// effectively focusing it
        /// </summary>
        /// <param name="surface">The surface to focus</param>
        public void Focus (FucksSurfaceManager surface) {
            int idx = surfaces.FindIndex ((obj) => obj == surface);
            if (idx == surfaces.Count - 1) return;
            surfaces.Swap (idx, surfaces.Count - 1);
            dirty = true;
        }

        /// <summary>
        /// Removes a surface
        /// </summary>
        /// <param name="surfaceIndex">the index of the surface to remove</param>
        public void RemoveSurface (int surfaceIndex) {
            surfaces.RemoveAt (surfaceIndex);
        }

        /// <summary>
        /// Translates a surface along the XY plane
        /// </summary>
        /// <param name="manager">the surface</param>
        /// <param name="x">amount to move in X</param>
        /// <param name="y">amount to move in Y</param>
        public void Translate (FucksSurfaceManager manager, int x, int y) {
            invalidated = true;
            manager.Translate (x, y);
        }
        /// <summary>
        /// Clears a cell unconditionally
        /// </summary>
        /// <param name="opnorm">the position of the cell</param>
        private void ClearCell (TermPosition opnorm) {
            Console.ResetColor ();
            Console.SetCursorPosition (opnorm.Y, opnorm.X);
            Console.Write (' ');
        }

        /// <summary>
        /// Renders one iteration of all surfaces
        /// </summary>
        /// <param name="all">whether to redraw all surfaces</param>
        public void renderOnce (bool all = false) {
            dirty = dirty || surfaces.Any(x => x.Dirty); 
            Console.CursorVisible = false;
            Console.SetCursorPosition (0, 0); //set it to 0,0 to disallow pointless movement
            TermSize sz = TermSize.CurrentTermSize;
            if (sz != currentInfo.size) {
                currentInfo.size = sz;
                renderState = new GlobalTermState[sz.X, sz.Y]; //TODO Realloc instead?
                invalidated = true;
            }
            all = all || invalidated;
            if (invalidated) {
                Console.ResetColor ();
                Console.Clear ();
            }
            TermPosition position;
            int surfaceCount = surfaces.Count;
            for (position.X = 0; position.X < sz.X; position.X++)
                for (position.Y = 0; position.Y < sz.Y; position.Y++) {
                    if (!dirty && !renderState[position.X, position.Y].dirty && renderState[position.X, position.Y].rendered) continue;
                    for (int i = 0; i < surfaceCount; i++) {
                        var sf = surfaces[i];
                        var rst = sf.RenderIfInBounds (position, all);
                        if (rst != RenderState.NotInBounds) {
                            if (rst == RenderState.IgnoreIfPossible)
                                if (i == surfaceCount - 1) {
                                    // ask it to render with a default background color
                                    sf.RenderIfInBounds (position, all, true);
                                }
                            else continue;
                            for (int x = 0; x < sf.currentState.resolution.Xscale; x++)
                                for (int y = 0; y < sf.currentState.resolution.Yscale; y++) {
                                    ref
                                    var rs = ref renderState[position.X + x, position.Y + y];
                                    rs.rendered = true;
                                    rs.dirty = false;
                                }
                            break;
                        }
                    }
                    ref
                    var rs0 = ref renderState[position.X, position.Y];
                    if (!rs0.rendered) {
                        ClearCell (position);
                        rs0.rendered = true;
                        rs0.dirty = false;
                    }
                }
            invalidated = false;
            Console.CursorVisible = true;
        }
    }
}