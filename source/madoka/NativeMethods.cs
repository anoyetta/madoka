using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace madoka
{
    public static class NativeMethods
    {
        public enum PROCESS_DPI_AWARENESS
        {
            /// <summary>
            /// DPIスケーリング非対応
            /// </summary>
            PROCESS_DPI_UNAWARE = 0,

            /// <summary>
            /// DPIスケーリング対応だが異なるDPIのモニタには対応していない
            /// </summary>
            PROCESS_SYSTEM_DPI_AWARE = 1,

            /// <summary>
            /// DPIスケーリング対応
            /// </summary>
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width => this.Right - this.Left;
            public int Height => this.Top - this.Bottom;
        }

        [DllImport("User32.Dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hwnd, ref RECT rc);

        [Flags]
        public enum SWPFlags : uint
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000,
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        public enum ShowState : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);

        public static IntPtr FindWindow(
            string processName)
        {
            var handle = IntPtr.Zero;

            var callback = new EnumWindowsDelegate((hWnd, _) =>
            {
                const bool NEXT = true;

                int processId;
                GetWindowThreadProcessId(hWnd, out processId);
                if (processId == 0)
                {
                    return NEXT;
                }

                var p = Process.GetProcessById(processId);
                if (p == null)
                {
                    return NEXT;
                }

                if (p.ProcessName.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    handle = hWnd;
                    return false;
                }

                return NEXT;
            });

            EnumWindows(callback, IntPtr.Zero);

            return handle;
        }
    }
}
