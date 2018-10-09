using nFucks;

namespace nFucksTest {
    class MainClass {
        static TermSize resv = TermSize.CurrentTermSize; //TrySetTerminalSize(160, 400);
        // get a manager
        static FucksManager fucksManager = new FucksManager ();
        public static void Main (string[] args) {
            if (false && resv.Y >= 80) {
                DrawYohane ();
                return;
            }
            // get a surface at 8,10 with size of 10x40 real characters
            var surface0 = fucksManager.CreateAndInitializeSurface (new TermPosition (8, 10), new TermResolution (10, 80, 1, 2));
            // get a surface at 0,0 with a scaled size (two real characters per Y cell) of 10x20 characters, and set the "skipped" cells to ' '
            var surface1 = fucksManager.CreateAndInitializeSurface (
                TermPosition.Origin,
                new TermResolution (10, 40, 1, 2),
                new char[, ] { { ' ', FucksSurfaceManager.FillValue } }
            );
            surface1.defaultProvider = new StaticTermColorProvider(new string[] { "122", "245", "43" }, new string[] { "12", "145", "143" });
            // set the default color scheme of the first surface (foreground is black, background is gray)
            surface0.defaultProvider = new StaticTermColorProvider (new string[] { "0" }, new string[] { "8" });

            // write "Hello, world" starting at 1,2 of the first surface
            var pos = new TermPosition (1, 2);
            //surface0.PutString ("Hello, World", ref pos);
            // make the 'd' background green
            surface0.SetBackColor (pos, BasicColor.Green);

            // put a '!' after the 'd'
            pos.advanceRight (surface0.bounds);
            //surface0.PutChar ('!', pos);

            // put the string "hane" starting at 2,3 of the second surface
            pos.Set (2, 3);
            surface1.PutString ("hane", ref pos);

            // put a '*' where the 'e' would be if it were rendered in the first surface
            //surface0.PutChar ('*', ref pos);

            // draw a frame for both windows
            surface0.drawBounds ();
            surface1.drawBounds ();
            TermPosition termPosition = new TermPosition (1, 1);
            // render one iteration

            // burrow three cells and spread them in a nice surface next to each other
            using (var tempSurface = surface1.burrowCells (true, new TermPosition (2, 2), new TermPosition (3, 3), new TermPosition (4, 4))) {
                // write "Yo" "ha" "ne" in those individual cells
                var posv = TermPosition.Origin;
                tempSurface.PutString ("Yohane", ref posv);
                // the cells will be automatically merged back into the main surface when this block ends
            }
            using (var tempSurface = surface0.burrowCells(false, Utils.GenerateArc(5, 10, 3, 0, 359)))
            {
                var posv = TermPosition.Origin;
                tempSurface.PutString("  c i r   c l e h e l   l o !", ref posv);
            }
            fucksManager.renderOnce ();
            while (true) {
                // intercept a key from the terminal
                var key = System.Console.ReadKey (true);

                // move the surface around if it's an arrow key
                // otherwise just write it to the surface
                switch (key.Key) {
                    case System.ConsoleKey.UpArrow:
                        fucksManager.Translate (surface0, -1, 0);
                        break;
                    case System.ConsoleKey.DownArrow:
                        fucksManager.Translate (surface0, 1, 0);
                        break;
                    case System.ConsoleKey.LeftArrow:
                        fucksManager.Translate (surface0, 0, -1);
                        break;
                    case System.ConsoleKey.RightArrow:
                        fucksManager.Translate (surface0, 0, 1);
                        break;
                    default:
                        fucksManager.PutChar (surface0, key.KeyChar, ref termPosition);
                        break;
                }
                // render all the surfaces
                fucksManager.renderOnce ();
            }
        }
        public static void DrawYohane () {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "/yohane.color";
            var origin = TermPosition.Origin;
            var surface0 = fucksManager.CreateAndInitializeSurface (origin, new TermResolution (36, 60));
            using (var sr = new System.IO.StreamReader (path))
                surface0.PutIRCColoredString (sr.ReadToEnd (), ref origin);
            fucksManager.renderOnce ();
            System.Console.ReadKey ();
        }
    }
}