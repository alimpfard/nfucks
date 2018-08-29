using nFucks;

namespace nFucksTest
{
    class MainClass
    {
		static TermSize resv = TermSize.CurrentTermSize;
		// get a manager
		static FucksManager fucksManager = new FucksManager();
        public static void Main(string[] args)
        {
			// get a surface at 8,10 with size of 10x40 real characters
			var surface0 = fucksManager.CreateAndInitializeSurface(new TermPosition(8, 10), new TermResolution(10, 40));
			// get a surface at 0,0 with a scaled size (two real characters per Y cell) of 10x20 characters
			var surface1 = fucksManager.CreateAndInitializeSurface(new TermPosition(0, 0), new TermResolution(10, 40, 1, 2));
			// set the default color scheme of the first surface (foreground is black, background is gray)
			surface0.defaultProvider = new StaticTermColorProvider(System.ConsoleColor.Black, System.ConsoleColor.Gray);

			// write "Hello, world" starting at 1,2 of the first surface
			var pos = new TermPosition(1, 2);
			surface0.PutString("Hello, World", ref pos);
			// make the 'd' background green
			surface0.SetBackColor(pos, BasicColor.Green);

			// put a '!' after the 'd'
			pos.advanceRight(surface0.bounds);
			surface0.PutChar('!', pos);

			// put the string "what" starting at 2,2 of the second surface
			pos.Set(2, 2);
			surface1.PutString("what", ref pos);

			// put a '*' where the 't' would be if it were rendered in the first surface
			surface0.PutChar('*', ref pos);

			// draw a frame for both windows
			surface0.drawBounds();
			surface1.drawBounds();

			// render one iteration
			fucksManager.renderOnce();
			for (int i=0; i<100; i++)
            {
				System.Console.ReadKey();
				// focus the first surface and render it
                fucksManager.Focus(surface0);
                fucksManager.renderOnce();
				System.Console.ReadKey();

				// focus the second surface, move it around and render it again
                fucksManager.Focus(surface1);
                // move the surface around a bit            
				fucksManager.Translate(surface1, (i % 3 - 1), (i % 3 - 1) * -1);
				fucksManager.renderOnce();
            }
		}
    }
}
