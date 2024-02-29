using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TransferEncodeDecode.Helpers
{
    /// <summary>
    /// As in the actual program Window rather then the 'Windows' operating system... Used to focus windows (bring to front etc)
    /// </summary>
    public class WindowHelper
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(int hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool BringWindowToTop(HandleRef hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, uint Msg);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

        public static extern int DeregisterShellHookWindow(IntPtr hWnd);
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        // Define the callback delegate's type.
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, UInt32 lParam);

        private static readonly int SW_RESTORE = 9;
        private static readonly int SW_NORMAL = 5;
        private static readonly int SW_MAXIMIZE = 3;
        private static readonly int SW_MINIMIZE = 6;
        private static readonly int SW_FORCEMINIMIZE = 11;

        private static readonly uint SW_RESTORE_HEX = 0x09;
        private static readonly uint SC_RESTORE = 0xF120;

        private static readonly uint WM_SYSCOMMAND = 0x0112;

        // Save window titles and handles in these lists.
        private static List<IntPtr> WindowHandles;
        private static List<string> WindowTitles;

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        public enum WindowPlacement
        {
            Normal,
            Minimised,
            Maximised,
            Restored,
            CantFind
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            return rect;
        }

        public static IntPtr FindWindow(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow)
        {
            return FindWindowEx(hwndParent, hwndChildAfter, lpszClass, lpszWindow);
        }

        public static IntPtr FindWindowEx(string ipClassName, string ipWindowName)
        {
            return FindWindow(ipClassName, ipWindowName);
        }

        /// <summary>
        /// Return a list of the desktop windows' handles and titles.
        /// </summary>
        public static void GetDesktopWindowHandlesAndTitles(out List<IntPtr> handles, out List<string> titles)
        {
            WindowHandles = new List<IntPtr>();
            WindowTitles = new List<string>();

            if (!EnumDesktopWindows(IntPtr.Zero, FilterCallback,
                IntPtr.Zero))
            {
                handles = null;
                titles = null;
            }
            else
            {
                handles = WindowHandles;
                titles = WindowTitles;
            }
        }

        /// <summary>
        /// We use this function to filter windows. This version selects visible windows that have titles.
        /// </summary>
        private static bool FilterCallback(IntPtr hWnd, int lParam)
        {
            // Get the window's title.
            StringBuilder sb_title = new StringBuilder(1024);
            int _ = GetWindowText(hWnd, sb_title, sb_title.Capacity);
            string title = sb_title.ToString();

            // If the window is visible and has a title, save it.
            if (IsWindowVisible(hWnd) &&
                string.IsNullOrEmpty(title) == false)
            {
                WindowHandles.Add(hWnd);
                WindowTitles.Add(title);
            }

            // Return true to indicate that we
            // should continue enumerating windows.
            return true;
        }

        /// <summary>
        /// Get process Id from handle
        /// </summary>
        public int GetProcessIdFromWindowHandle(IntPtr handle)
        {
            var _ = GetWindowThreadProcessId(handle, out uint processId);
            return (int)processId;
        }

        /// <summary>
        /// Get all windows handles of a process
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static IntPtr[] GetProcessWindows(int processId)
        {
            IntPtr[] apRet = (new IntPtr[256]);
            int iCount = 0;
            IntPtr pLast = IntPtr.Zero;
            do
            {
                pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
                GetWindowThreadProcessId(pLast, out uint iProcess_);
                if (iProcess_ == processId) apRet[iCount++] = pLast;
            } while (pLast != IntPtr.Zero);
            System.Array.Resize(ref apRet, iCount);
            return apRet;
        }

        /// <summary>
        /// Sometimes the process object's windows handle is zero - This is a workaround to get it.
        /// </summary>
        public static string FindWindowTitleFromProcessIdAndASetOfWindowHandlesAndTitles(int processId, List<IntPtr> handles, List<string> titles)
        {
            IntPtr[] processWindowHandles = GetProcessWindows(processId);
            foreach (IntPtr processWindow in processWindowHandles)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    if (handles[i] == processWindow)
                        return titles[i];
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Sometimes the process object's windows handle is zero - This is a workaround to get it.
        /// </summary>
        internal static string[] FindWindowTitlesFromProcessIdAndASetOfWindowHandlesAndTitles(int processId, List<IntPtr> handles, List<string> titles)
        {
            var foundTitles = new List<string>();
            IntPtr[] processWindowHandles = GetProcessWindows(processId);
            foreach (IntPtr processWindow in processWindowHandles)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    if (handles[i] == processWindow)
                        foundTitles.Add(titles[i]);
                }
            }

            return foundTitles.ToArray();
        }

        /// <summary>
        /// Sometimes the process object's windows handle is zero - This is a workaround to get it
        /// </summary>
        public static string FindWindowTitleFromProcessId(int processId)
        {
            GetDesktopWindowHandlesAndTitles(
                            out List<IntPtr> handles,
                            out List<string> titles);

            IntPtr[] processWindowHandles = GetProcessWindows(processId);
            foreach (IntPtr processWindow in processWindowHandles)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    if (handles[i] == processWindow)
                        return titles[i];
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sometimes the process object's windows handle is zero - This is a workaround to get it
        /// </summary>
        internal static string[] FindWindowTitlesFromProcessId(int processId)
        {
            var foundTitles = new List<string>();
            GetDesktopWindowHandlesAndTitles(
                            out List<IntPtr> handles,
                            out List<string> titles);

            IntPtr[] processWindowHandles = GetProcessWindows(processId);
            foreach (IntPtr processWindow in processWindowHandles)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    if (handles[i] == processWindow)
                        foundTitles.Add(titles[i]);
                }
            }

            return foundTitles.ToArray();
        }

        /// <summary>
        /// Move and / or resize window
        /// </summary>
        public static void MoveWindow(Process process, int x, int y, int width, int hight)
        {
            MoveWindow(process.MainWindowHandle, x, y, width, hight, true);
        }

        /// <summary>
        /// Move and / or resize window
        /// </summary>
        public static void MoveWindow(IntPtr handle, int x, int y, int width, int hight)
        {
            MoveWindow(handle, x, y, width, hight, true);
        }

        /// <summary>
        /// Makes sure to show the window. If it's minimised - Restore it. If it's behind then Active it. If it's of a protected process, attach to this process to active it...
        /// </summary>
        public static void ShowRestoreAndFocusWindow(IntPtr handle)
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
        public static bool IsWindowMinimised(IntPtr handle)
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
        public static bool IsWindowMaximised(IntPtr handle)
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
        /// Based on window handle - Get the windows placement (normal, minimised, maximised)
        /// </summary>
        public static WindowPlacement GetWindowPlacement(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(handle, ref placement);
                switch (placement.showCmd)
                {
                    case 1:
                        return WindowPlacement.Normal;
                    case 2:
                        return WindowPlacement.Minimised;
                    case 3:
                        return WindowPlacement.Maximised;
                }
            }

            return WindowPlacement.CantFind;
        }

        /// <summary>
        /// Based on window handle - Set the windows placement (normal, minimised, maximised)
        /// </summary>
        public static void SetWindowPlacement(IntPtr handle, WindowPlacement windowPlacement)
        {
            switch (windowPlacement)
            {
                case WindowPlacement.Normal:
                    ShowWindow(handle, (uint)SW_NORMAL);
                    ShowWindowAsync(handle, SW_NORMAL);
                    break;
                case WindowPlacement.Restored:
                    ShowWindow(handle, (uint)SW_RESTORE);
                    ShowWindowAsync(handle, SW_RESTORE);
                    break;
                case WindowPlacement.Minimised:
                    ShowWindow(handle, (uint)SW_MINIMIZE);
                    ShowWindowAsync(handle, SW_MINIMIZE);
                    ShowWindow(handle, (uint)SW_FORCEMINIMIZE);
                    ShowWindowAsync(handle, SW_FORCEMINIMIZE);
                    break;
                case WindowPlacement.Maximised:
                    ShowWindow(handle, (uint)SW_MAXIMIZE);
                    ShowWindowAsync(handle, SW_MAXIMIZE);
                    break;
                case WindowPlacement.CantFind:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Based on window handle - Set the windows to minimised
        /// </summary>
        public static void MinimiseWindow(IntPtr handle)
        {
            ShowWindow(handle, (uint)SW_MINIMIZE);
            ShowWindowAsync(handle, SW_MINIMIZE);
            ShowWindow(handle, (uint)SW_FORCEMINIMIZE);
            ShowWindowAsync(handle, SW_FORCEMINIMIZE);
        }

        /// <summary>
        /// Based on window handle - Set the windows to maxomised
        /// </summary>
        public static void MaximiseWindow(IntPtr handle)
        {
            ShowWindow(handle, (uint)SW_MAXIMIZE);
            ShowWindowAsync(handle, SW_MAXIMIZE);
        }

        /// <summary>
        /// Based on window handle - Set the windows to restored
        /// </summary>
        public static void RestoreWindow(IntPtr handle)
        {
            ShowWindow(handle, (uint)SW_RESTORE);
            ShowWindowAsync(handle, SW_RESTORE);
        }

        /// <summary>
        /// Windows Terminal helper class
        /// </summary>
        public class WindowsTerminalHelper
        {
            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            private const uint SWP_NOZORDER = 0x0004;

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            public static void SetHeight(int height, int processId = -1)
            {
                SetSize(-1, height, processId);
            }

            public static void SetWidth(int width, int processId = -1)
            {
                SetSize(width, -1, processId);
            }
            public static void SetSize(int width = -1, int height = -1, int processId = -1)
            {
                try
                {
                    Process process;
                    if (processId != -1)
                        process = Process.GetCurrentProcess();
                    else
                        process = Process.GetProcessById(processId);

                    if (process.ProcessName.Contains("WindowsTerminal"))
                    {
                        try
                        {
                            IntPtr hWnd = process.MainWindowHandle;
                            if (hWnd != IntPtr.Zero)
                            {
                                if (GetWindowRect(hWnd, out RECT rect))
                                {
                                    if (height == -1)
                                        height = rect.Bottom - rect.Top;

                                    if (width == -1)
                                        width = rect.Right - rect.Left;

                                    SetWindowPos(hWnd, IntPtr.Zero, rect.Left, rect.Top, width, height, SWP_NOZORDER);
                                }
                            }
                            else
                                SetWindowsTerminalsWidth(width);
                        }
                        catch
                        {
                            SetWindowsTerminalsWidth(width);
                        }
                    }
                    else
                        SetWindowsTerminalsWidth(width);

                }
                catch { }
            }

            private static void SetWindowsTerminalsWidth(int width)
            {
                var windowsTerminalProcesses = Process.GetProcessesByName("WindowsTerminal");
                if (windowsTerminalProcesses != null && windowsTerminalProcesses.Length > 0)
                {
                    foreach (var process in windowsTerminalProcesses)
                    {
                        try
                        {
                            IntPtr hWnd = process.MainWindowHandle;

                            if (hWnd != IntPtr.Zero)
                            {
                                if (GetWindowRect(hWnd, out RECT rect))
                                {
                                    int height = rect.Bottom - rect.Top;
                                    SetWindowPos(hWnd, IntPtr.Zero, rect.Left, rect.Top, width, height, SWP_NOZORDER);

                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public class ChildWindowHandler
        {
            private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

            [DllImport("user32")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

            private readonly IntPtr _MainHandle;

            public ChildWindowHandler(IntPtr handle)
            {
                this._MainHandle = handle;
            }

            /// <summary>
            /// Retrieve all child handles from a main handle
            /// </summary>
            public List<IntPtr> GetAllChildHandles()
            {
                List<IntPtr> childHandles = new List<IntPtr>();

                GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
                IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

                try
                {
                    EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                    EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
                }
                finally
                {
                    gcChildhandlesList.Free();
                }

                return childHandles;
            }

            private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
            {
                GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

                if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
                {
                    return false;
                }

                List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
                childHandles.Add(hWnd);

                return true;
            }
        }
    }
}