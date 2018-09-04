using System;
namespace nFucks {
    public abstract class ITermAPI {
        public abstract ConsoleColor BackgroundColor { get; set; }
        public abstract ConsoleColor ForegroundColor { get; set; }
        public abstract void ResetColor ();
        public abstract void Write<T> (T c);
        public abstract void SetCursorPosition (int x, int y);
        public abstract TermSize GetSize ();
        public virtual void Clear () {
            TermSize sz = GetSize ();
            int c_x = 0, c_y = 0;
            for (; c_y < sz.Y; c_y++)
                for (c_x = 0; c_x < sz.X; c_x++) {
                    SetCursorPosition (c_x, c_y); // sensible default; override
                    Write (' ');
                }
        }
    }
    public class ConsoleTermAPI : ITermAPI {
        public override ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
        public override ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public override void Clear () => Console.Clear ();
        public override TermSize GetSize () => TermSize.CurrentTermSize;
        public override void ResetColor () => Console.ResetColor ();
        public override void SetCursorPosition (int x, int y) => Console.SetCursorPosition (x, y);
        public override void Write<T> (T c) => Console.Write (c);
    }
}