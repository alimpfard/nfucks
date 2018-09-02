using System;
using static nFucks.FucksManager;

namespace nFucks {
	public partial class FucksSurfaceManager {
		readonly int MAX_HISTORY = 4;
		public TermState [] stepBacks;
		public TermState currentState;
		public ITermColor defaultProvider;
		public TermPosition surfacePosition;
		public TermSize bounds;
		private bool dirty = false;
		private bool forced_dirty = false;
		private TermPosition positionDelta;
		public const char FillValue = (char)0;
		private int dirty_count;
		private bool resizeWithConsole = false;

		public TermPosition PositionDelta { get => positionDelta; private set => positionDelta = value; }
		public bool Dirty { get => dirty; }
        public bool ResizeWithConsole { get => resizeWithConsole; private set => resizeWithConsole = value; }

        public delegate void RefAction<T1> (ref T1 arg1);

		#region Constructors
		public FucksSurfaceManager ()
		{
			stepBacks = new TermState [MAX_HISTORY];
			bounds = TermSize.CurrentTermSize;
			var res = new TermResolution (bounds.X, bounds.Y, 1, 1);
			currentState = new TermState (res);
			defaultProvider = BasicColor.Default;
		}
		public FucksSurfaceManager (TermResolution resolution)
		{
			stepBacks = new TermState [MAX_HISTORY];
			currentState = new TermState (resolution);
			defaultProvider = BasicColor.Default;
		}
		public FucksSurfaceManager (TermResolution resolution, ITermColor dp)
		{
			stepBacks = new TermState [MAX_HISTORY];
			currentState = new TermState (resolution);
			bounds = TermSize.CurrentTermSize;
			defaultProvider = dp;
		}
		public FucksSurfaceManager (TermPosition spos, TermSize sz, ITermColor dp)
		{
			stepBacks = new TermState [MAX_HISTORY];
			surfacePosition = spos;
			TermResolution resolution = new TermResolution (sz.X, sz.Y);
			currentState = new TermState (resolution);
			bounds = sz;
			defaultProvider = dp;
		}
		public FucksSurfaceManager (TermPosition spos, TermSize sz, TermResolution resolution, ITermColor dp)
		{
			stepBacks = new TermState [MAX_HISTORY];
			surfacePosition = spos;
			currentState = new TermState (resolution);
			bounds = sz;
			defaultProvider = dp;
		}
		#endregion Constructors

		/// <summary>
		/// Initialize this instance.
		/// (Fills the cell buffer)
		/// this may be skipped
		/// </summary>
		public void Initialize ()
		{
			for (int x = 0; x < bounds.X; x++)
				for (int y = 0; y < bounds.Y; y++) {
					dirty_count++;
					ref var cell = ref currentState.cells [x, y];
					cell.Data = ' ';
					cell.FillPattern = new char [currentState.resolution.Xscale, currentState.resolution.Yscale];
					for (int i = 0; i < currentState.resolution.Xscale; i++)
						for (int j = 0; j < currentState.resolution.Yscale; j++)
							cell.FillPattern [i, j] = i == j && i == 0 ? FillValue : ' ';
				}
		}
		internal void RecalculateFillPatterns ()
		{
			for (int x = 0; x < bounds.X; x++)
				for (int y = 0; y < bounds.Y; y++) {
					dirty_count++;
					ref var cell = ref currentState.cells [x, y];
					var fpls = cell.FillPattern;
					var fpl0 = fpls?.GetLength (0);
					var fpl1 = fpls?.GetLength (1);
					cell.FillPattern = new char [currentState.resolution.Xscale, currentState.resolution.Yscale];
					for (int i = 0; i < currentState.resolution.Xscale; i++)
						for (int j = 0; j < currentState.resolution.Yscale; j++) {
							cell.FillPattern [i, j] = fpl0 == null || fpl1 == null ? (i == j && i == 0 ? FillValue : ' ') : i < fpl0 && j < fpl1 ? fpls [i, j] : ' ';
						}
				}
		}

		/// <summary>
		/// Initialize this instance, set the fill pattern of all cells to <c>pat</c>
		/// </summary>
		/// <param name="pat">The fill pattern of the cells</param>
		public void Initialize (char [,] pat)
		{
			for (int x = 0; x < bounds.X; x++)
				for (int y = 0; y < bounds.Y; y++) {
					ref var cell = ref currentState.cells [x, y];
					cell.Data = ' ';
					cell.FillPattern = (char [,])pat.Clone ();
				}
		}

		/// <summary>
		/// Burrows some select cells and spreads them in a <code>[resolution.Xscale, count*resolution.Yscale]</code> grid of <code>[1, 1]</code> cells
		/// </summary>
		/// <param name="pos">any number of cell positions on the surface</param>
		/// <returns>an IDisposable surface-esque object</returns>
		public WithBurrowedCell burrowCells (params TermPosition [] pos)
		{
			return new WithBurrowedCell (ref currentState.cells, ref currentState.resolution, pos);
		}

		/// <summary>
		/// Burrows some select cells and spreads them in a <code>[resolution.Xscale, count*resolution.Yscale]</code> grid of <code>[1, 1]</code> cells
		/// </summary>
		/// <param name="pos">any number of cell positions on the surface</param>
		/// <param name="new_pat">whether to ignore <i>all</i> the old patterns</param>
		/// <returns>an IDisposable surface-esque object</returns>
		public WithBurrowedCell burrowCells (bool new_pat, params TermPosition [] pos)
		{
			return new WithBurrowedCell (ref currentState.cells, ref currentState.resolution, pos, new_pat);
		}

		/// <summary>
		/// Ask for a redraw at next draw call
		/// </summary>
		public void MarkDirty ()
		{
			dirty = true;
			forced_dirty = true;
		}

		public void MarkCellDirtyIfInBounds(TermPosition pos) 
		{
			if (RaytraceLocal(ref pos)) {
				currentState.cells[pos.X, pos.Y].dirty = true;
				dirty_count++;
			}
		}

		/// <summary>
		/// set the surface clean
		/// </summary>
		public void MarkClean ()
		{
			dirty = false;
			forced_dirty = false;
			dirty_count = 0;
		}

		/// <summary>
		/// Push back one state to the history
		/// </summary>
		/// <param name="termState">state to push back.</param>
		public void PushStateBack (TermState termState)
		{
			stepBacks [0] = stepBacks [1];
			stepBacks [1] = stepBacks [2];
			stepBacks [2] = stepBacks [3];
			stepBacks [3] = termState;
		}

		internal void Scale (int xscaled, int yscaled)
		{
			bounds.ScaleUp (xscaled, yscaled);
			currentState.Scale (xscaled, yscaled);
			RecalculateFillPatterns ();
		}

		/// <summary>
		/// Push one state to the history, from the front
		/// </summary>
		/// <param name="termState">state to push.</param>
		public void PushStateFront (TermState termState)
		{
			stepBacks [3] = stepBacks [2];
			stepBacks [2] = stepBacks [1];
			stepBacks [1] = stepBacks [0];
			stepBacks [0] = termState;
		}
		/// <summary>
		/// Pops a history state
		/// </summary>
		/// <returns>The state.</returns>
		public TermState PopState ()
		{
			var qv = stepBacks [3];
			PushStateFront (stepBacks [0]);
			return qv;
		}
		/// <summary>
		/// Shifts a history state.
		/// </summary>
		/// <returns>The state.</returns>
		public TermState ShiftState ()
		{
			var qv = stepBacks [0];
			PushStateBack (stepBacks [3]);
			return qv;
		}
		/// <summary>
		/// Puts a character into a cell complex
		/// </summary>
		/// <param name="c">Character to put.</param>
		/// <param name="position">Local (surface-wide) position of the cell.</param>
		public void PutChar (char c, TermPosition position)
		{
			ref var cells = ref currentState.cells;
			ref var cell = ref cells [position.X, position.Y];
			cell.Data = c;
			if (cell.dirty) {
				dirty = true;
				dirty_count++;
			}
		}

		/// <summary>
		/// Puts a character into a cell complex
		/// </summary>
		/// <param name="c">Character to put.</param>
		/// <param name="position">(reference) Local (surface-wide) position of the cell.</param>
		public void PutChar (char c, ref TermPosition position)
		{
			ref var cells = ref currentState.cells;
			ref var cell = ref cells [position.X, position.Y];
			cell.Data = c;
			if (cell.dirty) {
				dirty = true;
				dirty_count++;
			}
			if (c == '\n' || c == '\r')
				position.advanceDown (bounds);
			else
				position.advanceRight (bounds);
		}

		/// <summary>
		/// Sets the background color of a cell.
		/// </summary>
		/// <param name="position">Local position of the cell.</param>
		/// <param name="back">Color.</param>
		public void SetBackColor (TermPosition position, ITermColor back)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells [position.X, position.Y];
			cell.backgroundColor = back;
			cell.dirty = true;
			// currentState.cells[position.X, position.Y] = cell;
		}

		/// <summary>
		/// Sets the foreground color of a cell.
		/// </summary>
		/// <param name="position">Local position of the cell.</param>
		/// <param name="back">Color.</param>
		public void SetForeColor (TermPosition position, ITermColor back)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells [position.X, position.Y];
			cell.foregroundColor = back;
			cell.dirty = true;
			// currentState.cells[position.X, position.Y] = cell;
		}

		/// <summary>
		/// Translates this surface along the terminal
		/// </summary>
		/// <param name="x"> Translation along the X axis</param>
		/// <param name="y"> Translation along the Y axis</param>
		public void Translate (int x, int y)
		{
			surfacePosition.Translate (x, y);
			PositionDelta.Set (x, y);
			dirty = true;
			forced_dirty = true;
			CheckBoundsValid ();
		}

		private void CheckBoundsValid ()
		{
			if (surfacePosition.X < 0 || surfacePosition.Y < 0)
				throw new InvalidOperationException ("Translation to invalid bounds");
		}

		/// <summary>
		/// Sets the fill pattern of a cell (a pattern of <code>FucksSurfaceManager.FillValue</code> means to put the value of the cell here
		/// </summary>
		/// <param name="pattern">A char matrix pattern</param>
		/// <param name="pos">position of the cell</param>
		public void SetFillPattern (char [,] pattern, TermPosition pos)
		{
			ref var cell = ref currentState.cells [pos.X, pos.Y];
			cell.FillPattern = pattern;
			dirty = dirty || cell.dirty;
		}

		/// <summary>
		/// Spreads a string across the surface 
		/// </summary>
		/// <param name="s">The string.</param>
		/// <param name="pos">Starting position.</param>
		public void PutString (string s, ref TermPosition pos)
		{
			int strlen = s.Length;
			if (strlen == 0) return;
			int str_idx = 0;
			int c_x;
			for (c_x = pos.Y; pos.X < currentState.resolution.Xres; pos.X++) {
				for (; pos.Y < currentState.resolution.Yres; pos.Y++) {
					if (str_idx == strlen) { pos.Y--; return; }
					PutChar (s [str_idx++], pos);
				}
				if (str_idx == strlen) {
					pos.Y--;
					return;
				}
				pos.Y = c_x;
			}
		}

		/// <summary>
		/// Puts a string from top to bottom, 
		/// only filling from the current position onwards
		/// </summary>
		/// <param name="s">String to put in buffer</param>
		/// <param name="pos">Start position</param>
		public void PutVertString (string s, ref TermPosition pos)
		{
			int strlen = s.Length;
			if (strlen == 0) return;
			int str_idx = 0;
			int c_x;
			for (c_x = pos.X; pos.Y < currentState.resolution.Yres; pos.Y++) {
				for (; pos.X < currentState.resolution.Xres; pos.X++) {
					if (str_idx == strlen) { pos.X--; return; }
					PutChar (s [str_idx++], pos);
				}
				if (str_idx == strlen) {
					pos.X--;
					return;
				}
				pos.X = c_x;
			}
		}

		/// <summary>
		/// Puts a string vertically from top to bottom
		/// </summary>
		/// <param name="s">String to put</param>
		/// <param name="pos">Start position</param>
		public void PutReverseVertString (string s, ref TermPosition pos)
		{
			int strlen = s.Length;
			if (strlen == 0) return;
			int str_idx = 0;
			int c_x;
			for (c_x = pos.X; pos.Y >= 0; pos.Y--) {
				for (; pos.X >= 0; pos.X--) {
					if (str_idx == strlen) { pos.X++; return; }
					PutChar (s [str_idx++], pos);
				}
				if (str_idx == strlen) {
					pos.X++;
					return;
				}
				pos.X = c_x;
			}
		}

		/// <summary>
		/// renders a cell unless the background color/foreground color is set to transparent
		/// </summary>
		/// <param name="cell">The cell to render</param>
		/// <param name="position">Global scale position to render in</param>
		/// <param name="xs">X scale of the current cell complex</param>
		/// <param name="ys">Y scale of the current cell complex</param>
		/// <param name="notrans">if set to <c>true</c> will ignore transparency</param>
		/// <param name="force">force the cell to be rendered</param>
		/// <returns>whether the cell was rendered</returns>
		private bool renderCell (ref TermCell cell, ref TermPosition position, int xs, int ys, bool notrans = false, bool force = false)
		{
			cell.dirty = false;
			ITermColor back = cell.backgroundColor, fore = cell.foregroundColor;
			ConsoleColor? rf;
			if (back != null)
				rf = back.AsConsoleColor ();
			else
				rf = defaultProvider.ProvideFallback (position);
			if (rf == null && !notrans) return false;
			Console.BackgroundColor = rf ?? Console.BackgroundColor; // don't touch it
			if (fore != null)
				rf = fore.AsConsoleColor ();
			else
				rf = defaultProvider.ProvideFallback (position, true);
			char data = cell.Data;
			if (rf == null && notrans) return false;
			if (rf == null) data = (char)0;
			Console.ForegroundColor = rf ?? Console.ForegroundColor; // don't touch it
			TermPosition normalized = (position + surfacePosition).ScaledUp (xs, ys);
			for (int i = 0; i < xs; i++)
				for (int j = 0; j < ys; j++) {
					Console.SetCursorPosition (normalized.Y + j, normalized.X + i);
					char fill = cell.FillPattern [i, j];
					if (fill == FillValue) fill = data;
					Console.Write (fill);
				}
			return true;
		}

		/// <summary>
		/// Checks if a pseudo-local position is in bounds of this surface
		/// </summary>
		/// <param name="position">The position to check</param>
		/// <returns>whether the position is in bounds</returns>
		public bool LocalInBounds (TermPosition position)
		{
			return position.X >= 0 && position.X < bounds.X && position.Y >= 0 && position.Y < bounds.Y;
		}

		/// <summary>
		/// Raytraces a global position, and modifies it to match the local position
		/// </summary>
		/// <returns><c>true</c>, if raytrace hit, <c>false</c> otherwise.</returns>
		/// <param name="position">Position to trace; contents unspecified if raytrace fails.</param>
		public bool RaytraceLocal (ref TermPosition position)
		{
			position = position - surfacePosition;
			position.ScaledDown (currentState.resolution.Xscale, currentState.resolution.Yscale);
			// position holds the pseudo-local position of the hit, if it's inbounds, we have a hit
			return LocalInBounds (position);
		}

		/// <summary>
		/// Tries to render the cell at a global position if it is in bounds
		/// </summary>
		/// <param name="position">The global position of the cell</param>
		/// <param name="force">whether to forefully render the cell</param>
		/// <param name="notrans">whether to ignore transparency</param>
		/// <param name="redraw">whether to redraw the cell without regards to anything</param>
		/// <returns>the status of the render</returns>
		public RenderState RenderIfInBounds (TermPosition position, bool force = false, bool notrans = false, bool redraw = false)
		{
			if (RaytraceLocal (ref position)) {
				ref var cell = ref currentState.cells [position.X, position.Y];
				if (!(force || forced_dirty || cell.dirty || dirty_count > 0 || redraw)) return RenderState.Skipped;
				dirty_count--;
				if (!renderCell (ref cell, ref position, currentState.resolution.Xscale, currentState.resolution.Yscale, notrans, redraw))
					return RenderState.IgnoreIfPossible; //it was a "hole" 
				return RenderState.Rendered;
			}
			return RenderState.NotInBounds;
		}

		/// <summary>
		/// Draws the bounds of the surface.
		/// <warning>Will overwrite any characters that are on the inner bounds</warning>
		/// </summary>
		public void drawBounds ()
		{
			TermSize sz = bounds; //.Scale(currentState.resolution.Xscale, currentState.resolution.Yscale);
			string hor = new string ('-', sz.Y - 1), ver = new string ('|', sz.X - 1);
			TermPosition pos = new TermPosition (0, 0);
			PutChar ('+', ref pos);
			PutString (hor, ref pos);
			PutChar ('+', ref pos);
			/* +-----------------+
			 *>.
			 */
			PutVertString (ver, ref pos);
			/* +-----------------+
			 * |
			 * .<
			 */
			PutChar ('+', ref pos);
			/*  +-----------------+
			 *  |
			 *  +.<
			 */
			PutString (hor, ref pos);
			/* +-----------------+
			 *  |
			 * +------------------<
			 */
			PutChar ('+', pos);
			/* +-----------------+
			 *  |
			 * +-----------------+<
			 */
			pos.advanceUp (sz);
			PutReverseVertString (ver.Substring (1), ref pos);
			/* +-----------------+
			 *  |                      | <
			 * +-----------------+
			 */
		}

		public override string ToString ()
		{
			return $"Surface{{{surfacePosition}-{bounds}}}";
		}
	}
}
