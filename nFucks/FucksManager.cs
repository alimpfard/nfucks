using System;
using System.Collections.Generic;
using System.Linq;

namespace nFucks
{
	public class FucksManager
	{
		List<FucksSurfaceManager> surfaces = new List<FucksSurfaceManager>();
		private bool invalidated = false;
		private bool dirty = false;
		private GlobalTermInfo currentInfo = new GlobalTermInfo(TermSize.CurrentTermSize);
		private GlobalTermState[,] renderState;
		private Natives natives = new Natives();
		public FucksManager()
		{
			natives.SetConsoleRaw();
			renderState = new GlobalTermState[currentInfo.size.X, currentInfo.size.Y];
		}
		~FucksManager()
		{
			natives.RestoreConsoleMode();
		}
		public FucksSurfaceManager CreateSurface(TermPosition position, TermResolution resolution)
		{
			var sz = new TermSize(resolution.Xres, resolution.Yres);
			var manager = new FucksSurfaceManager(position, sz, resolution, BasicColor.Default);
			surfaces.Add(manager);
			return manager;
		}

		public FucksSurfaceManager CreateAndInitializeSurface(TermPosition position, TermResolution resolution)
		{
			var sz = new TermSize(resolution.Xres, resolution.Yres);
			var manager = new FucksSurfaceManager(position, sz, resolution, BasicColor.Default);
			manager.Initialize();
			surfaces.Add(manager);
			return manager;
		}

		public void Focus(FucksSurfaceManager surface)
		{
			int idx = surfaces.FindIndex((obj) => obj == surface);
			if (idx == surfaces.Count - 1) return;
			surfaces.Swap(idx, surfaces.Count - 1);
			dirty = true;
		}

		public void RemoveSurface(int surfaceIndex)
		{
			surfaces.RemoveAt(surfaceIndex);
		}

		public void Translate(FucksSurfaceManager manager, int x, int y)
		{
			invalidated = true;
			manager.Translate(x, y);
		}

		/// <summary>
		/// Clears the translation residues [terminal is lazily drawn, so these won't be cleared otherwise]
		/// </summary>
		internal void ClearTranslationResidues(FucksSurfaceManager surface)
		{
			if (surface.PositionDelta.Size == 0) return;
			var oldpos = surface.surfacePosition.CompoundMaxmimumBound(surface.PositionDelta);
			int xs = surface.currentState.resolution.Xscale, ys = surface.currentState.resolution.Yscale;
			TermPosition opnorm = oldpos.ScaleUp(xs, ys), curnorm = (surface.surfacePosition.CompoundMaxmimumBound(surface.bounds)).ScaledUp(xs, ys);
			bool rendered = false;
			for (int i = opnorm.X; i <= curnorm.X + 1; i++)
				for (int j = opnorm.Y; j <= curnorm.Y + 1; j++)
				{
					opnorm.Set(i, j);
					foreach (var sf in surfaces)
					{
						if (sf == surface) continue;
						if (sf.RenderIfInBounds(opnorm) != RenderState.NotInBounds) { rendered = true; break; }
					}
					if (!rendered)
						ClearCell(opnorm);
				}
		}

		private void ClearCell(TermPosition opnorm)
		{
			Console.ResetColor();
			Console.SetCursorPosition(opnorm.Y, opnorm.X);
			Console.Write(' ');
		}

		public void renderOnce(bool all = false)
		{
			Console.CursorVisible = false;
			Console.SetCursorPosition(0, 0); //set it to 0,0 to disallow pointless movement
			TermSize sz = TermSize.CurrentTermSize;
			if (sz != currentInfo.size)
			{
				currentInfo.size = sz;
				renderState = new GlobalTermState[sz.X, sz.Y]; //TODO Realloc instead?
				invalidated = true;
			}
			all = all || invalidated;
			if (invalidated)
			{
				Console.ResetColor();
				Console.Clear();
			}
			TermPosition position;
			int surfaceCount = surfaces.Count;
			for (position.X = 0; position.X < sz.X; position.X++)
				for (position.Y = 0; position.Y < sz.Y; position.Y++)
				{
					if (!dirty && !renderState[position.X, position.Y].dirty && renderState[position.X, position.Y].rendered) continue;
					for (int i = 0; i < surfaceCount; i++)
					{
						var sf = surfaces[i];
						var rst = sf.RenderIfInBounds(position, all);
						if (rst != RenderState.NotInBounds)
						{
							if (rst == RenderState.IgnoreIfPossible)
								if (i == surfaceCount - 1) 
								{
									// ask it to render with a default background color
									sf.RenderIfInBounds(position, all, true);
								}
								else continue;
							for (int x = 0; x < sf.currentState.resolution.Xscale; x++)
								for (int y = 0; y < sf.currentState.resolution.Yscale; y++)
								{
									ref var rs = ref renderState[position.X + x, position.Y + y];
									rs.rendered = true;
									rs.dirty = false;
								}
							break;
						}
					}
					ref var rs0 = ref renderState[position.X, position.Y];
					if (!rs0.rendered)
					{
						ClearCell(position);
						rs0.rendered = true;
						rs0.dirty = false;
					}
				}
			invalidated = false;
			Console.CursorVisible = true;
		}
	}
}
