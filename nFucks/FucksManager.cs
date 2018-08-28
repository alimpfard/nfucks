using System;
using System.Collections.Generic;

namespace nFucks
{
	public class FucksManager
	{
		List<FucksSurfaceManager> surfaces = new List<FucksSurfaceManager>();
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

		public void renderOnce(bool all = false)
		{
			foreach (var surface in surfaces)
			{
				surface.renderOnce(all);
			}
		}
	}
}
