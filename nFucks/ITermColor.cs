namespace nFucks
{
    public class Defaults
    {
        public static string ToANSIString(string[] selection, bool fg)
        {
            string form = "\u001b[";
            form += fg ? "38;" : "48;";
            if (selection.Length == 1)
                form += "5;" + selection[0];
            else
            {
                form += "2";
                for (int i = 0; i < 3; i++)
                    form += $";{selection[i]}";
            }
        return form + "m";
        }
    }
    public interface ITermColor
    {
        string ProvideFallback(TermPosition position, bool foreground = false);
        string[] ANSIEscapes(bool foreground = false);
        string AsANSIEscapeCode(bool foreground = false);
    }
    public class AbstractTermColorProvider : ITermColor
    {
        public virtual string[] ANSIEscapes(bool foreground = false)
        {
            throw new System.NotImplementedException();
        }

        public virtual string AsANSIEscapeCode(bool foreground = false)
        {
            throw new System.NotImplementedException();
        }

        public virtual string ProvideFallback(TermPosition position, bool foreground) =>
            throw new System.Exception();
    }
    public class TermColorProvider : AbstractTermColorProvider
    {
        public readonly System.Func<TermPosition, bool, string> provider;
        public TermColorProvider(System.Func<TermPosition, bool, string> pr)
        {
            provider = pr;
        }

        public override string[] ANSIEscapes(bool foreground = false)
        {
            return new string[] { };
        }

        public override string AsANSIEscapeCode(bool foreground = false)
        {
            return ProvideFallback(TermPosition.Origin, foreground);
        }

        public override string ProvideFallback(TermPosition position, bool foreground)
        {
            return provider(position, foreground);
        }

    }
    public class StaticTermColorProvider : AbstractTermColorProvider
    {
        string[] fore, back;
        public StaticTermColorProvider(string[] f0, string[] f1)
        {
            fore = f0;
            back = f1;
        }
        public override string ProvideFallback(TermPosition position, bool foreground)
        {
            return AsANSIEscapeCode(foreground);
        }
        public override string[] ANSIEscapes(bool foreground = false)
        {
            return foreground ? fore : back;
        }
        public override string AsANSIEscapeCode(bool foreground = false)
        {
            return Defaults.ToANSIString(foreground ? fore : back, foreground);
        }
    }
    public class StaticSingleTermColorProvider : AbstractTermColorProvider
    {
        string[] color;
        public StaticSingleTermColorProvider(string[] f0)
        {
            color = f0;
        }
        public StaticSingleTermColorProvider(int[] f0)
        {
            // TODO
        }
        public override string[] ANSIEscapes(bool foreground = false)
        {
            return base.ANSIEscapes(foreground);
        }

        public override string AsANSIEscapeCode(bool foreground = false)
        {
            return Defaults.ToANSIString(color, foreground);
        }

        public override string ProvideFallback(TermPosition position, bool foreground)
        {
            return AsANSIEscapeCode(foreground);
        }

    }
}