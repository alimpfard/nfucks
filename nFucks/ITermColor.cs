namespace nFucks
{
	public interface ITermColor
	{
		char[] ColorCodes(bool foreground = false);
		System.ConsoleColor? AsConsoleColor();
		System.ConsoleColor? ProvideFallback(TermPosition position, bool foreground = false);
	}
	public class AbstractTermColorProvider : ITermColor
    {
		public virtual char[] ColorCodes(bool foreground) => throw new System.Exception();
		public virtual System.ConsoleColor? AsConsoleColor() => throw new System.Exception();
		public virtual System.ConsoleColor? ProvideFallback(TermPosition position, bool foreground) => throw new System.Exception();
    }
	public class TermColorProvider : AbstractTermColorProvider
	{
		public readonly System.Func<TermPosition, bool, System.ConsoleColor> provider;
		public TermColorProvider (System.Func<TermPosition, bool, System.ConsoleColor> pr)
		{
			provider = pr;
		}
		public override System.ConsoleColor? ProvideFallback(TermPosition position, bool foreground)
		{
			return provider(position, foreground);
		}
	}
	public class StaticTermColorProvider : AbstractTermColorProvider
	{
		System.ConsoleColor? fore, back;
		public StaticTermColorProvider(System.ConsoleColor? f0, System.ConsoleColor? f1)
		{
			fore = f0;
			back = f1;
		}
		public override System.ConsoleColor? ProvideFallback(TermPosition position, bool foreground)
        {
			return foreground ? fore : back;
        }
	}
}