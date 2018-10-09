using System;
using System.Collections.Generic;

namespace nFucks {
    public partial class FucksManager {
        List<FucksSurfaceManager> surfaces = new List<FucksSurfaceManager> ();
        private bool invalidated = false;
        private bool dirty = false;
        private GlobalTermState[, ] renderState;
        private ITermAPI termAPI;
        private GlobalTermInfo currentInfo;
        private Natives natives = new Natives ();

        public FucksManager () {
            //natives.SetConsoleRaw ();
            termAPI = new ConsoleTermAPI ();
            currentInfo = new GlobalTermInfo (termAPI.GetSize ());
            renderState = new GlobalTermState[currentInfo.size.X, currentInfo.size.Y];
        }
        public FucksManager (ITermAPI api) {
                termAPI = api;
                currentInfo = new GlobalTermInfo (termAPI.GetSize ());
                renderState = new GlobalTermState[currentInfo.size.X, currentInfo.size.Y];
            }

            ~FucksManager () {
                // natives.RestoreConsoleMode ();
                termAPI.ResetColor ();
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

        /// <summary>
        /// Creates and initializes a surface encompasses the whole terminal
        /// </summary>
        /// <returns>The surface</returns>
        public FucksSurfaceManager CreateAndInitializeFullSurface () {
            var sz = termAPI.GetSize ();
            var manager = new FucksSurfaceManager (TermPosition.Origin, sz, BasicColor.Default);
            manager.Initialize ();
            surfaces.Add (manager);
            return manager;
        }

        /// <summary>
        /// Creates and initializes a surface
        /// </summary>
        /// <param name="position">the top-left corner of the surface, in global terms</param>
        /// <param name="resolution">the resolution of the surface</param>
        /// <param name="v">the fill pattern of the surface</param>
        /// <returns></returns>
        public FucksSurfaceManager CreateAndInitializeSurface (TermPosition position, TermResolution resolution, char[, ] v) {
            var sz = new TermSize (resolution.Xres, resolution.Yres);
            var manager = new FucksSurfaceManager (position, sz, resolution, BasicColor.Default);
            manager.Initialize (v);
            surfaces.Add (manager);
            return manager;
        }

        public void PutChar (FucksSurfaceManager surface, char c, ref TermPosition termPosition) {
            surface.PutChar (c, ref termPosition);
            dirty = surface.Dirty;
        }

        public void PutChar (FucksSurfaceManager surface, char c, TermPosition termPosition) {
            surface.PutChar (c, ref termPosition);
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
            surfaces.ForEach (mgr => mgr.MarkDirty ()); //XXX this redraws the whole frame
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
            // invalidated = true;
            var postl = manager.surfacePosition;
            var bound = manager.bounds;
            manager.Translate (x, y);
            postl = postl.CompoundMinimumBound (x, y);
            var posbr = bound.CompoundMaximumBound (x, y) + postl;
            for (TermPosition pos = postl; pos.X <= posbr.X; pos.X++)
                for (pos.Y = postl.Y; pos.Y <= posbr.Y; pos.Y++) {
                    ref
                    var v = ref renderState[pos.X, pos.Y];
                    v.rendered = false;
                    v.dirty = true;
                    surfaces.ForEach (mgr => mgr.MarkCellDirtyIfInBounds (pos));
                }
        }
        /// <summary>
        /// Clears a cell unconditionally
        /// </summary>
        /// <param name="opnorm">the position of the cell</param>
        private void ClearCell (TermPosition opnorm) {
            termAPI.ResetColor ();
            termAPI.SetCursorPosition (opnorm.Y, opnorm.X);
            termAPI.Write (' ');
        }

        /// <summary>
        /// Renders one iteration of all surfaces
        /// </summary>
        /// <param name="all">whether to redraw all surfaces</param>
        public void renderOnce (bool all = false) {
            dirty = dirty || surfaces.Any (x => x.Dirty);
            termAPI.SetCursorPosition (0, 0);
            TermSize sz = termAPI.GetSize ();
            if (sz != currentInfo.size) {
                int Xscaled = sz.X / currentInfo.size.X;
                int Yscaled = sz.Y / currentInfo.size.Y;
                currentInfo.size = sz;
                renderState = new GlobalTermState[sz.X, sz.Y]; //TODO Realloc instead?
                foreach (var surface in surfaces) {
                    if (surface.ResizeWithConsole) {
                        surface.Scale (Xscaled, Yscaled);
                    }
                    surface.MarkDirty ();
                }
                invalidated = true;
            }
            if (invalidated) {
                termAPI.ResetColor ();
                termAPI.Clear ();
                clearRenderState ();
                // TODO clear translation artefacts
            }
            all = all || invalidated;
            TermPosition position;
            int surfaceCount = surfaces.Count;
            for (position.X = 0; position.X < sz.X; position.X++)
                for (position.Y = 0; position.Y < sz.Y; position.Y++) {
                    var should_render = renderState[position.X, position.Y].dirty || !renderState[position.X, position.Y].rendered || invalidated;
                    if (!(all || dirty || should_render))
                        continue;
                    for (int i = 0; i < surfaceCount; i++) {
                        var sf = surfaces[i];
                        ref
                        var resolution = ref sf.currentState.resolution;
                        var rst = sf.RenderIfInBounds (termAPI, position, should_render, redraw : should_render);
                        if (rst != RenderState.NotInBounds) {
                            if (rst == RenderState.IgnoreIfPossible)
                                if (i == surfaceCount - 1) {
                                    // ask it to render with a default background color
                                    sf.RenderIfInBounds (termAPI, position, all, true, should_render);
                                }
                            else continue;
                            for (int x = 0; x < resolution.Xscale; x++)
                                for (int y = 0; y < resolution.Yscale; y++) {
                                    ref var rs = ref renderState[position.X + x, position.Y + y];
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
            surfaces.ForEach (sf => sf.MarkClean ());
        }

        private void clearRenderState () {
            foreach (var srf in surfaces) {
                var st = srf.surfacePosition;
                var bo = srf.bounds.Scale (srf.currentState.resolution.Xscale, srf.currentState.resolution.Yscale);
                int y = st.Y;
                for (; st.X < bo.X; st.X++)
                    for (st.Y = y; st.Y < bo.Y; st.Y++) {
                        ref
                        var rs = ref renderState[st.X, st.Y];
                        rs.rendered = false;
                        rs.dirty = true;
                    }
            }
        }

        public override string ToString () {
            return surfaces.ToString ();
        }
    }
}