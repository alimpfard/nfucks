namespace nFucks {
    public struct TermCell
    {
        private char data;
        //public int metadata;
        public bool dirty;
        public ITermColor backgroundColor, foregroundColor;
        private char[,] fillPattern;
        public char[,] FillPattern
        {
            get => fillPattern;
            set
            {
                if (fillPattern == value) return;
                fillPattern = value;
                dirty = true;
            }
        }
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
        public char UnsafeData
        {
            get => data;
            set => data = value;
        }
    }
}