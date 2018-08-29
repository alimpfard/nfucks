using System;
using System.Runtime.InteropServices;

namespace nFucks {
    public class Natives {
#if _WINDOWS
        [DllImport ("kernel32.dll")]
        static extern bool SetConsoleMode (IntPtr hConsoleHandle, uint dwMode);
        [DllImport ("kernel32.dll")]
        static extern bool GetConsoleMode (IntPtr hConsoleHandle, out uint lpMode);
        [DllImport ("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle (int nStdHandle);
        const int STD_OUTPUT_HANDLE = -11;
        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr (-1);

        uint ConsoleModePreRaw;

        [Flags]
        private enum ConsoleModes : uint {
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_AUTO_POSITION = 0x0100,
            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
        }

        public bool SetConsoleRaw () {
            IntPtr hOut = GetStdHandle (STD_OUTPUT_HANDLE);
            if (hOut != INVALID_HANDLE_VALUE) {
                uint mode;
                if (GetConsoleMode (hOut, out mode)) {
                    ConsoleModePreRaw = mode;
                    mode &= (uint) ~ConsoleModes.ENABLE_ECHO_INPUT;
                    SetConsoleMode (hOut, mode);
                    return true;
                }
            }
            return false;
        }
        public bool RestoreConsoleMode () {
            IntPtr hOut = GetStdHandle (STD_OUTPUT_HANDLE);
            if (hOut != INVALID_HANDLE_VALUE) {
                SetConsoleMode (hOut, ConsoleModePreRaw);
                return true;
            }
            return false;
        }
#else
        public bool SetConsoleRaw () {
            return true; // assume it worked
        }
        public bool RestoreConsoleMode () {
            return true; // assume it worked
        }
#endif
    }
}