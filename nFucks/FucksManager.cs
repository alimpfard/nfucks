using System;
using System.Collections.Generic;
using System.Linq;

namespace nFucks
{
	public class FucksManager
	{
		List<FucksSurfaceManager> surfaces = new List<FucksSurfaceManager>();
		private bool invalidated = false;
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
			surface.MarkDirty();
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
						if (sf.RenderIfInBounds(opnorm)) { rendered = true; break; }
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
			all = all || invalidated;
			if (invalidated)
			{
				Console.ResetColor();
				Console.Clear();
			}
			foreach (var surface in surfaces)
			{
				surface.renderOnce(all);
			}
			invalidated = false;
		}
	}
}
