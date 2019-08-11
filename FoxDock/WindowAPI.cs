using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace FoxDock
{
    class WindowAPI
    {
        public static MainWindow window = new MainWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

        public static bool IsOnDesktop()
        {
            // Get all the processes.
            Process[] proc = Process.GetProcesses();

            // Get current window handle.
            IntPtr cWin = GetForegroundWindow();

            foreach (Process x in proc)
            {
                if (x.MainWindowHandle == cWin)
                {
                    var placement = GetPlacement(cWin);
                    //Debug.WriteLine(placement.showCmd.ToString());
                    if (placement.showCmd.ToString() == "Maximized" || IsFullScreen(cWin))
                        return false;
                }
            }

            return true;
        }
        public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static System.Windows.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        /// <summary>
        /// Shows the desktop.
        /// </summary>
        public static void ShowDesktop()
        {
            keybd_event(0x5B, 0, 0, 0);
            keybd_event(0x4D, 0, 0, 0);
            keybd_event(0x5B, 0, 0x2, 0);
        }

        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOP = new IntPtr(-1);

        const int HC_ACTION = 0;
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        static IntPtr HookHandle = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, KbHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        static IntPtr IgnoreWin_D(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HC_ACTION
                && IsWin_D(wParam, lParam))
            {
                ShowDesktop();
                return (IntPtr)1; //just ignore the key press
            }


            return CallNextHookEx(HookHandle, nCode, wParam, lParam);
        }

        static bool IsWin_D(IntPtr wParam, IntPtr lParam)
        {
            if ((int)wParam != WM_KEYDOWN)
                return false;

            var keyInfo = (KbHookParam)Marshal.PtrToStructure(lParam, typeof(KbHookParam));
            if (keyInfo.VkCode != (int)System.Windows.Forms.Keys.D) return false;
            return GetAsyncKeyState(System.Windows.Forms.Keys.LWin) < 0
                   || GetAsyncKeyState(System.Windows.Forms.Keys.RWin) < 0;
        }

        delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct KbHookParam
        {
            public readonly int VkCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr Extra;
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr Hwnd);

        [DllImport("USER32.DLL")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public const int SW_MAXIMIZE = 1;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

        public static int GetPlacementState(string strstate)
        {
            int state = 1;
            switch (strstate)
            {
                case "Hidden":
                    state = 0;
                    break;
                case "Normal":
                    state = 1;
                    break;
                case "Minimized":
                    state = 2;
                    break;
                case "Maximized":
                    state = 3;
                    break;
            }
            return state;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hP, IntPtr hC, string sC, string sW);
        const int GWL_HWNDPARENT = -8;
        public static void MakeWin()
        {
            IntPtr hprog = FindWindowEx(
                FindWindowEx(
                    FindWindow("Progman", "Program Manager"),
                    IntPtr.Zero, "SHELLDLL_DefView", ""
                ),
                IntPtr.Zero, "SysListView32", "FolderView"
            );
            try
            {
                if (window != null)
                {
                    IntPtr Handle = new WindowInteropHelper(window).Handle;

                    SetWindowLong(Handle, GWL_HWNDPARENT, hprog);
                }
            }
            catch
            {

            }
            

        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        public static void SendToBack(Window window)
        {
            var hWnd = new WindowInteropHelper(window).Handle;
            WindowAPI.SetWindowPos(hWnd, WindowAPI.HWND_BOTTOM, 0, 0, 0, 0, WindowAPI.SWP_NOSIZE | WindowAPI.SWP_NOMOVE);

        }
        public static void SendToTop(Window window)
        {
            var hWnd = new WindowInteropHelper(window).Handle;
            WindowAPI.SetWindowPos(hWnd, WindowAPI.HWND_TOP, 0, 0, 0, 0, WindowAPI.SWP_NOSIZE | WindowAPI.SWP_NOMOVE);

        }
        public enum TaskBarLocation { TOP, BOTTOM, LEFT, RIGHT }

        public static TaskBarLocation GetTaskBarLocation()
        {
            //System.Windows.SystemParameters....
            if (SystemParameters.WorkArea.Left > 0)
                return TaskBarLocation.LEFT;
            if (SystemParameters.WorkArea.Top > 0)
                return TaskBarLocation.TOP;
            if (SystemParameters.WorkArea.Left == 0
              && SystemParameters.WorkArea.Width < SystemParameters.PrimaryScreenWidth)
                return TaskBarLocation.RIGHT;
            return TaskBarLocation.BOTTOM;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        public static bool IsFullScreen(IntPtr intPtr)
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen;

            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, intPtr), ref rect);
            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top).Contains(screen.Bounds);
        }
        public const UInt32 SPI_GETMOUSESPEED = 0x0070;

        [DllImport("User32.dll")]
        public static extern Boolean SystemParametersInfo(
            UInt32 uiAction,
            UInt32 uiParam,
            IntPtr pvParam,
            UInt32 fWinIni);

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private static IntPtr _MainHandle;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();
        enum GetAncestorFlags
        {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function. 
            /// </summary>
            GetParent = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GetRoot = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent. 
            /// </summary>
            GetRootOwner = 3
        }
        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public static bool KeepWindowHandleInAltTabList(IntPtr w)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(w, ref placement);
            string cmd = placement.showCmd.ToString();
            if (cmd == "Minimized" ||  cmd == "Maximized") return true;
            return false;
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
        {
            IntPtr lastPopUp = GetLastActivePopup(window);
            if (IsWindowVisible(lastPopUp))
                return lastPopUp;
            else if (lastPopUp == window)
                return IntPtr.Zero;
            else
                return GetLastVisibleActivePopUpOfWindow(lastPopUp);
        }
        public static List<IntPtr> GetAllChildHandles(IntPtr handl)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(handl, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
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

        public enum RecycleFlag : int

        {

            SHERB_NOCONFIRMATION = 0x00000001, // No confirmation, when emptying

            SHERB_NOPROGRESSUI = 0x00000001, // No progress tracking window during the emptying of the recycle bin

            SHERB_NOSOUND = 0x00000004 // No sound when the emptying of the recycle bin is complete

        }
        [DllImport("Shell32.dll")]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlag dwFlags);
    }


}
