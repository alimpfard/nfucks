namespace nFucks {
    public struct TermCell
    {
        private char data;
        //public int metadata;
        public bool dirty;
        public ITermColor backgroundColor, foregroundColor;
        public char Data
        {
            get => data;
            set
            {
                if (value == '\r' || value == '\n') value = ' ';
                if (data != value)
                {
                    data = value;
                    dirty = true;
                }
            }
        }
    }
}