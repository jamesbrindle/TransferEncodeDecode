using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TransferEncodeDecode.Helpers
{
    /// <summary>
    /// As in the actual program Window rather then the 'Windows' operating system... Used to focus windows (bring to front etc)
    /// </summary>
    internal class WindowHelper
    {
        [DllImport("user32.dll")]
        internal static extern int SetForegroundWindow(int hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(HandleRef hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        internal static extern int ShowWindow(IntPtr hWnd, uint Msg);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        // Define the callback delegate's type.
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, UInt32 lParam);

        private static readonly int SW_RESTORE = 9;
        private static readonly int SW_MAXIMIZE = 3;

        private static readonly uint SW_RESTORE_HEX = 0x09;
        private static readonly uint SC_RESTORE = 0xF120;

        private static readonly uint WM_SYSCOMMAND = 0x0112;

        private struct WINDOWPLACEMENT
        {
            internal int length;
            internal int flags;
            internal int showCmd;
            internal System.Drawing.Point ptMinPosition;
            internal System.Drawing.Point ptMaxPosition;
            internal System.Drawing.Rectangle rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };


        /// <summary>
        /// Makes sure to show the window. If it's minimised - Restore it. If it's behind then Active it. If it's of a protected process, attach to this process to active it...
        /// </summary>
        internal static void ShowRestoreAndFocusWindow(IntPtr handle)
        {
            bool complete = false;

            try
            {
                SetForegroundWindow(handle);
            }
            catch { }

            try
            {
                BringWindowToTop(handle);
            }
            catch { }

            try
            {
                if (IsWindowMinimised(handle))
                    ShowWindowAsync(handle, SW_RESTORE);
            }
            catch { }

            try
            {
                if (IsWindowMinimised(handle))
                    ShowWindow(handle, SW_RESTORE_HEX);
            }
            catch { }

            try
            {
                if (IsWindowMinimised(handle))
                    ShowWindowAsync(handle, SW_RESTORE);
            }
            catch { }

            try
            {
                if (IsWindowMinimised(handle))
                    ShowWindow(handle, SW_RESTORE_HEX);
            }
            catch { }

            try
            {
                if (IsWindowMinimised(handle))
                    SendMessage(handle, WM_SYSCOMMAND, SC_RESTORE, 0);
            }
            catch { }

            try
            {
                SetForegroundWindow(handle);
            }
            catch { }

            try
            {
                BringWindowToTop(handle);
            }
            catch { }

            try
            {
                FocusWindow(handle);
            }
            catch { }

            new Thread((ThreadStart)delegate
            {
                for (int i = 0; i < 8; i++)
                {
                    try
                    {
                        SetForegroundWindow(handle);
                    }
                    catch { }

                    try
                    {
                        BringWindowToTop(handle);
                    }
                    catch { }

                    Thread.Sleep(100);
                }

                complete = true;
            }).Start();

            int it = 0;
            while (!complete && it < 50)
            {
                Thread.Sleep(50);
                it++;
            }
        }

        private static void FocusWindow(IntPtr handle)
        {
            uint currentlyFocusedWindowProcessId = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            uint appThread = GetCurrentThreadId();

            if (currentlyFocusedWindowProcessId != appThread)
            {
                AttachThreadInput(currentlyFocusedWindowProcessId, appThread, true);
                BringWindowToTop(handle);
                AttachThreadInput(currentlyFocusedWindowProcessId, appThread, false);
                SetForegroundWindow(handle);
            }

            else
            {
                BringWindowToTop(handle);
                SetForegroundWindow(handle);
            }
        }

        /// <summary>
        /// Based on window handle - is the window minimised or not?
        /// </summary>
        internal static bool IsWindowMinimised(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(handle, ref placement);
                switch (placement.showCmd)
                {
                    case 2:
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Based on window handle - is the window maximised or not?
        /// </summary>
        internal static bool IsWindowMaximised(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(handle, ref placement);
                switch (placement.showCmd)
                {
                    case 3:
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Based on window handle - Set the windows to maxomised
        /// </summary>
        internal static void MaximiseWindow(IntPtr handle)
        {
            ShowWindow(handle, (uint)SW_MAXIMIZE);
            ShowWindowAsync(handle, SW_MAXIMIZE);
        }

        /// <summary>
        /// Based on window handle - Set the windows to restored
        /// </summary>
        internal static void RestoreWindow(IntPtr handle)
        {
            ShowWindow(handle, (uint)SW_RESTORE);
            ShowWindowAsync(handle, SW_RESTORE);
        }
    }
}