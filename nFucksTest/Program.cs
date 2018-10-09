using nFucks;

namespace nFucksTest {
    class MainClass {
        static TermSize resv = TermSize.CurrentTermSize; //TrySetTerminalSize(160, 400);
        // get a manager
        static FucksManager fucksManager = new FucksManager ();
        public static void Main (string[] args) {
			System.Console.ReadKey (true);
			System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint (0, 8086);
			System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket (System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.IP);
			// get a surface at 8,10 with size of 10x40 real characters
			var surfacernd = fucksManager.CreateAndInitializeSurface (TermPosition.Origin, new TermResolution (32, 64, 4, 8));
			var pos0 = TermPosition.Origin;
			socket.Bind (endPoint);
			socket.Listen (6);
			while (true) {
				var sock = socket.Accept ();
				byte[] board = new byte[64];
				if (sock.Receive (board, 64, System.Net.Sockets.SocketFlags.None) != 64) 
				{
					sock.Close ();
					continue;
				}
				for (int i = 0; i < 64; i++) {
					char c = (char) board [i];
					surfacernd.SetBackColor (pos0, c == 'X' ? BasicColor.Red : c == 'O' ? BasicColor.Blue : c == '.' ? BasicColor.Yellow : BasicColor.White);
					surfacernd.PutChar ((char)i, pos0);
					pos0.advanceRight (surfacernd.bounds);
				}
				fucksManager.renderOnce ();
			}
            /*if (resv.Y >= 80) {
                DrawYohane ();
                return;
            }*/
            // get a surface at 8,10 with size of 10x40 real characters
            var surface0 = fucksManager.CreateAndInitializeSurface (new TermPosition (8, 10), new TermResolution (10, 40));
            // get a surface at 0,0 with a scaled size (two real characters per Y cell) of 10x20 characters, and set the "skipped" cells to ' '
            var surface1 = fucksManager.CreateAndInitializeSurface (
                TermPosition.Origin,
                new TermResolution (10, 40, 1, 2),
                new char[, ] { { ' ', FucksSurfaceManager.FillValue } }
            );
            // set the default color scheme of the first surface (foreground is black, background is gray)
            surface0.defaultProvider = new StaticTermColorProvider (System.ConsoleColor.Black, System.ConsoleColor.Gray);

            // write "Hello, world" starting at 1,2 of the first surface
            var pos = new TermPosition (1, 2);
            surface0.PutString ("Hello, World", ref pos);
            // make the 'd' background green
            surface0.SetBackColor (pos, BasicColor.Green);

            // put a '!' after the 'd'
            pos.advanceRight (surface0.bounds);
            surface0.PutChar ('!', pos);

            // put the string "hane" starting at 2,3 of the second surface
            pos.Set (2, 3);
            surface1.PutString ("hane", ref pos);

            // put a '*' where the 'e' would be if it were rendered in the first surface
            surface0.PutChar ('*', ref pos);

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
            var surface0 = fucksManager.CreateAndInitializeSurface (origin, new TermResolution (resv.X, 160));
            using (var sr = new System.IO.StreamReader (path))
            surface0.PutIRCColoredString (sr.ReadToEnd (), ref origin);
            fucksManager.renderOnce ();
            System.Console.ReadKey ();
        }
    }
}
