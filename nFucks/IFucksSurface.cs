namespace nFucks
{
    interface IFucksSurface
    {
        void PutChar(char c, TermPosition position);
        void PutChar(char c, ref TermPosition position);
        void PutString(string s, ref TermPosition pos);
        void SetBackColor(TermPosition position, ITermColor back);
        void SetForeColor(TermPosition position, ITermColor back);
        void PutIRCColoredString(string s, ref TermPosition pos);
    }
}
