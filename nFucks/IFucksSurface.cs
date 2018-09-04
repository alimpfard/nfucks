namespace nFucks {
    interface IFucksSurface {

        void SetForeColor (TermPosition position, ITermColor back);
        void SetBackColor (TermPosition position, ITermColor back);
        void PutString (string s, ref TermPosition pos);
        void PutChar (char c, ref TermPosition position);
        void PutChar (char c, TermPosition position);
    }
}