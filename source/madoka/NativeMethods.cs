using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace madoka
{
    public static class NativeMethods
    {
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
            public int Height => this.Bottom - this.Top;
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

        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_READ = 0x0010;

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

        [DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        private const int S_OK = 0;

        public enum PROCESS_DPI_AWARENESS
        {
            /// <summary>DPIスケーリング非対応</summary>
            [Display(Name = "非対応")]
            PROCESS_DPI_UNAWARE = 0,

            /// <summary>DPIスケーリング対応だが異なるDPIのモニタには対応していない</summary>
            [Display(Name = "システムDPIスケーリング対応")]
            PROCESS_SYSTEM_DPI_AWARE = 1,

            /// <summary>DPIスケーリング対応</summary>
            [Display(Name = "システムDPIスケーリング対応＋マルチモニタ対応")]
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        [DllImport("Shcore.dll")]
        private static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

        public static IntPtr FindWindow(
            string processName)
        {
#if false

            var handle = IntPtr.Zero;

            var callback = new EnumWindowsDelegate((hWnd, _) =>
            {
                const bool NEXT = true;

                int processId;
                GetWindowThreadProcessId(hWnd, out processId);
                if (processId == 0)
                {
                    Thread.Yield();
                    return NEXT;
                }

                var p = Process.GetProcessById(processId);
                if (p == null)
                {
                    Thread.Yield();
                    return NEXT;
                }

                if (p.ProcessName.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    handle = hWnd;
                    return false;
                }

                Thread.Yield();
                return NEXT;
            });

            EnumWindows(callback, IntPtr.Zero);

            return handle;
#else
            var p = Process.GetProcessesByName(processName);
            if (p == null ||
                p.Length <= 0)
            {
                return IntPtr.Zero;
            }

            return p.First().MainWindowHandle;
#endif
        }

        public static PROCESS_DPI_AWARENESS GetDPIState(
            uint processId)
        {
            var handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);

            if (handle != IntPtr.Zero)
            {
                PROCESS_DPI_AWARENESS value;
                int result = GetProcessDpiAwareness(handle, out value);
                if (result == S_OK)
                {
                    System.Diagnostics.Debug.Print(value.ToString());
                }

                CloseHandle(handle);
                if (result != S_OK)
                {
                    throw new Win32Exception(result);
                }

                return value;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
