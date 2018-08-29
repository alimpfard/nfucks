namespace nFucks
{
	public class BasicColor : ITermColor
	{
		readonly char fcolor, bcolor;
		readonly System.ConsoleColor consoleColor;

		public readonly static BasicColor Black = new BasicColor(30, 40, System.ConsoleColor.Black);
		public readonly static BasicColor Red = new BasicColor(31, 41, System.ConsoleColor.Red);
		public readonly static BasicColor Green = new BasicColor(32, 42, System.ConsoleColor.Green);
		public readonly static BasicColor Yellow = new BasicColor(33, 43, System.ConsoleColor.Yellow);
		public readonly static BasicColor Blue = new BasicColor(34, 44, System.ConsoleColor.Blue);
		public readonly static BasicColor Magenta = new BasicColor(35, 45, System.ConsoleColor.Magenta);
		public readonly static BasicColor Cyan = new BasicColor(36, 46, System.ConsoleColor.Cyan);
		public readonly static BasicColor White = new BasicColor(37, 47, System.ConsoleColor.White);
		public readonly static BasicColor Default = new BasicColor();

		public BasicColor(int v1, int v2, System.ConsoleColor color)
		{
			fcolor = (char)v1;
			bcolor = (char)v2;
			consoleColor = color;
		}
		private BasicColor()
		{
		}
		public System.ConsoleColor? AsConsoleColor()
		{
			return consoleColor;
		}
		public char[] ColorCodes(bool foreground)
		{
			return new char[] { foreground ? fcolor : bcolor };
		}

		public System.ConsoleColor? ProvideFallback(TermPosition position, bool foreground)
		{
			return foreground ? System.ConsoleColor.Black : System.ConsoleColor.White;
		}
	}
}