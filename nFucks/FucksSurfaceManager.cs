using System;
namespace nFucks
{
	public class FucksSurfaceManager
	{
		readonly int MAX_HISTORY = 4;
		public TermState[] stepBacks;
		public TermState currentState;
		public ITermColor defaultProvider;
		public TermPosition surfacePosition;
		public TermSize bounds;
		private bool dirty = false;
		private bool forced_dirty = false;
		private TermPosition positionDelta;

		public TermPosition PositionDelta { get => positionDelta; private set => positionDelta = value; }

		public delegate void RefAction<T1>(ref T1 arg1);

		#region Constructors
		public FucksSurfaceManager()
		{
			stepBacks = new TermState[MAX_HISTORY];
			bounds = TermSize.CurrentTermSize;
			var res = new TermResolution(bounds.X, bounds.Y, 1, 1);
			currentState = new TermState(res);
			defaultProvider = BasicColor.Default;
		}
		public FucksSurfaceManager(TermResolution resolution)
		{
			stepBacks = new TermState[MAX_HISTORY];
			currentState = new TermState(resolution);
			defaultProvider = BasicColor.Default;
		}
		public FucksSurfaceManager(TermResolution resolution, ITermColor dp)
		{
			stepBacks = new TermState[MAX_HISTORY];
			currentState = new TermState(resolution);
			bounds = TermSize.CurrentTermSize;
			defaultProvider = dp;
		}
		public FucksSurfaceManager(TermPosition spos, TermSize sz, ITermColor dp)
		{
			stepBacks = new TermState[MAX_HISTORY];
			surfacePosition = spos;
			TermResolution resolution = new TermResolution(sz.X, sz.Y);
			currentState = new TermState(resolution);
			bounds = sz;
			defaultProvider = dp;
		}
		public FucksSurfaceManager(TermPosition spos, TermSize sz, TermResolution resolution, ITermColor dp)
		{
			stepBacks = new TermState[MAX_HISTORY];
			surfacePosition = spos;
			currentState = new TermState(resolution);
			bounds = sz;
			defaultProvider = dp;
		}
		#endregion Constructors

		/// <summary>
		/// Initialize this instance.
		/// (Fills the cell buffer)
		/// this may be skipped
		/// </summary>
		public void Initialize()
		{
			for (int x = 0; x < bounds.X; x++)
				for (int y = 0; y < bounds.Y; y++)
				{
					ref var cell = ref currentState.cells[x, y];
					cell.data = ' ';
					cell.dirty = true;
				}
		}

		/// <summary>
		/// Ask for a redraw at next draw call
		/// </summary>
		public void MarkDirty()
		{
			dirty = true;
			forced_dirty = true;
		}

		/// <summary>
		/// Push back one state to the history
		/// </summary>
		/// <param name="termState">state to push back.</param>
		public void PushStateBack(TermState termState)
		{
			stepBacks[0] = stepBacks[1];
			stepBacks[1] = stepBacks[2];
			stepBacks[2] = stepBacks[3];
			stepBacks[3] = termState;
		}
		/// <summary>
		/// Push one state to the history, from the front
		/// </summary>
		/// <param name="termState">state to push.</param>
		public void PushStateFront(TermState termState)
		{
			stepBacks[3] = stepBacks[2];
			stepBacks[2] = stepBacks[1];
			stepBacks[1] = stepBacks[0];
			stepBacks[0] = termState;
		}
		/// <summary>
		/// Pops a history state
		/// </summary>
		/// <returns>The state.</returns>
		public TermState PopState()
		{
			var qv = stepBacks[3];
			PushStateFront(stepBacks[0]);
			return qv;
		}
		/// <summary>
		/// Shifts a history state.
		/// </summary>
		/// <returns>The state.</returns>
		public TermState ShiftState()
		{
			var qv = stepBacks[0];
			PushStateBack(stepBacks[3]);
			return qv;
		}
		/// <summary>
		/// Puts a character into a cell complex
		/// </summary>
		/// <param name="c">Character to put.</param>
		/// <param name="position">Local (surface-wide) position of the cell.</param>
		public void PutChar(char c, TermPosition position)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells[position.X, position.Y];
			cell.data = c;
			cell.dirty = true;
			// currentState.cells[position.X, position.Y] = cell;
		}

		/// <summary>
		/// Puts a character into a cell complex
		/// </summary>
		/// <param name="c">Character to put.</param>
		/// <param name="position">(reference) Local (surface-wide) position of the cell.</param>
		public void PutChar(char c, ref TermPosition position)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells[position.X, position.Y];
			cell.data = c;
			cell.dirty = true;
			position.advance(bounds);
		}

		/// <summary>
		/// Sets the background color of a cell.
		/// </summary>
		/// <param name="position">Local position of the cell.</param>
		/// <param name="back">Color.</param>
		public void SetBackColor(TermPosition position, ITermColor back)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells[position.X, position.Y];
			cell.backgroundColor = back;
			cell.dirty = true;
			// currentState.cells[position.X, position.Y] = cell;
		}

		/// <summary>
		/// Sets the foreground color of a cell.
		/// </summary>
		/// <param name="position">Local position of the cell.</param>
		/// <param name="back">Color.</param>
		public void SetForeColor(TermPosition position, ITermColor back)
		{
			dirty = true;
			ref var cells = ref currentState.cells;
			ref var cell = ref cells[position.X, position.Y];
			cell.foregroundColor = back;
			cell.dirty = true;
			// currentState.cells[position.X, position.Y] = cell;
		}

		/// <summary>
		/// Translates this surface along the terminal
		/// </summary>
		/// <param name="x"> Translation along the X axis</param>
		/// <param name="y"> Translation along the Y axis</param>
		public void Translate(int x, int y)
		{
			var spl = surfacePosition;
			surfacePosition.Translate(x, y);
			PositionDelta.Set(x, y);
			dirty = true;
			forced_dirty = true;
			// CheckBoundsValid();
		}

		/// <summary>
		/// Spreads a string across the surface 
		/// </summary>
		/// <param name="s">The string.</param>
		/// <param name="pos">Starting position.</param>
		public void PutString(string s, ref TermPosition pos)
		{
			int strlen = s.Length;
			if (strlen == 0) return;
			int str_idx = 0;
			int c_x;
			for (c_x = pos.Y; pos.X < currentState.resolution.Xres; pos.X++)
			{
				for (; pos.Y < currentState.resolution.Yres; pos.Y++)
				{
					if (str_idx == strlen) { pos.Y--; return; }
					PutChar(s[str_idx++], pos);
				}
				if (str_idx == strlen)
				{
					pos.Y--;
					return;
				}
				pos.Y = c_x;
			}
		}

		private void renderCell(ref TermCell cell, ref TermPosition position, int xs, int ys)
		{
			cell.dirty = false;
			ITermColor back = cell.backgroundColor, fore = cell.foregroundColor;
			if (back != null)
				Console.BackgroundColor = back.AsConsoleColor();
			else
				Console.BackgroundColor = defaultProvider.ProvideFallback(position);
			if (fore != null)
				Console.ForegroundColor = fore.AsConsoleColor();
			else
				Console.ForegroundColor = defaultProvider.ProvideFallback(position, true);
			char data = cell.data;
			TermPosition normalized = (position + surfacePosition).ScaledUp(xs, ys);
			for (int i = 0; i < xs; i++)
				for (int j = 0; j < ys; j++)
				{
					Console.SetCursorPosition(normalized.Y + j, normalized.X + i);
					Console.Write(i + j == 0 ? data : ' ');
				}
		}

		/// <summary>
		/// Renders the surface.
		/// </summary>
		/// <param name="runForAll">If set to <c>true</c> will update all cells, regardless of them being marked dirty.</param>
		public void renderOnce(bool runForAll = false)
		{
			if (!dirty && !runForAll) return;
			runForAll = runForAll || forced_dirty;
			forced_dirty = false;
			Console.CursorVisible = false;
			ref var cells = ref currentState.cells;
			var xs = currentState.resolution.Xscale;
			var ys = currentState.resolution.Yscale;
			TermSize size = bounds;
			TermPosition position = new TermPosition(0, 0);
			for (; position.Y < size.Y; position.Y++)
				for (position.X = 0; position.X < size.X; position.X++)
				{
					ref TermCell cell = ref cells[position.X, position.Y];
					if (runForAll)
					{
						renderCell(ref cell, ref position, xs, ys);
						continue;
					}
					if (cell.dirty)
					{
						renderCell(ref cell, ref position, xs, ys);
					}
				}
			dirty = false;
			Console.CursorVisible = true;
			PushStateBack(currentState);
		}      

		public bool LocalInBounds(TermPosition position)
		{
			return position.X < bounds.X && position.Y < bounds.Y;
		}

        /// <summary>
        /// Raytraces a global position, and modifies it to match the local position
		/// 
        /// </summary>
        /// <returns><c>true</c>, if raytrace hit, <c>false</c> otherwise.</returns>
		/// <param name="position">Position to trace; contents unspecified if raytrace fails.</param>
		public bool RaytraceLocal(ref TermPosition position)
		{
			position = position - surfacePosition;
			position.ScaledDown(currentState.resolution.Xscale, currentState.resolution.Yscale);
			// position holds the pseudo-local position of the hit, if it's inbounds, we have a hit
			return LocalInBounds(position);
		}

		public bool RenderIfInBounds(TermPosition position)
		{
			if (RaytraceLocal(ref position))
			{
				renderCell(ref currentState.cells[position.X, position.Y], ref position, currentState.resolution.Xscale, currentState.resolution.Yscale);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Draws the bounds of the surface.
		/// <warning>Will overwrite any characters that are on the inner bounds</warning>
		/// </summary>
		public void drawBounds()
		{
			TermSize sz = bounds;//.Scale(currentState.resolution.Xscale, currentState.resolution.Yscale);
			string hor = new string('-', sz.Y), ver = new string('|', sz.X - 1);
			TermPosition pos = new TermPosition(0, 0);
			PutString(hor, ref pos);
			PutChar('+', pos);
			pos.advanceDown(bounds);
			PutString(ver, ref pos);
			PutChar('+', pos);
			pos = new TermPosition(sz.X - 1, 0);
			PutString(hor.Substring(1), ref pos);
		}

		public void runInternLoop(Action<char> onCharRead)
		{
			while (true)
			{
				renderOnce();
				if (Console.KeyAvailable)
					onCharRead(Console.ReadKey().KeyChar);
				System.Threading.Thread.Sleep(5);
			}
		}
	}
}
