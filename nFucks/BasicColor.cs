namespace nFucks {
    public class BasicColor : ITermColor {
        readonly char fcolor, bcolor;

        public readonly static BasicColor Black = new BasicColor (30, 40);
        public readonly static BasicColor Red = new BasicColor (31, 41);
        public readonly static BasicColor Green = new BasicColor (32, 42);
        public readonly static BasicColor Yellow = new BasicColor (33, 43);
        public readonly static BasicColor Blue = new BasicColor (34, 44);
        public readonly static BasicColor Magenta = new BasicColor (35, 45);
        public readonly static BasicColor Cyan = new BasicColor (36, 46);
        public readonly static BasicColor White = new BasicColor (37, 47);
        public readonly static BasicColor Default = new BasicColor ();

        public BasicColor (int v1, int v2) {
            fcolor = (char) v1;
            bcolor = (char) v2;
        }
        private BasicColor () { }

        string ITermColor.ProvideFallback(TermPosition position, bool foreground)
        {
            return AsANSIEscapeCode(foreground); 
        }

        public string[] ANSIEscapes(bool foreground = false)
        {
            return new string[] { (foreground ? fcolor : bcolor).ToString() };
        }

        public string AsANSIEscapeCode(bool foreground = false)
        {
            return Defaults.ToANSIString(ANSIEscapes(), foreground);
        }
    }
}