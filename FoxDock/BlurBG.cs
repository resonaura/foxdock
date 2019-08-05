using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FoxDock
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttribData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public AccentFlags AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [Flags]
        internal enum AccentFlags
        {
            // ... 
            DrawLeftBorder = 0x0,
            DrawTopBorder = 0x0,
            DrawRightBorder = 0x0,
            DrawBottomBorder = 0x0,
            DrawAllBorders = (DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder)
            // ... 
        }

        internal enum WindowCompositionAttribute
        {
            // ... 
            WCA_ACCENT_POLICY = 19
            // ... 
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }
        [Flags]
        enum DWM_BB
        {
            Enable = 1,
            BlurRegion = 2,
            TransitionMaximized = 4
        }
        [StructLayout(LayoutKind.Sequential)]
        struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;
            public int fEnable;
            public IntPtr hRgnBlur;
            public int fTransitionOnMaximized;

            public DWM_BLURBEHIND(bool enabled)
            {
                fEnable = enabled ? 1 : 0;
                hRgnBlur = IntPtr.Zero;
                fTransitionOnMaximized = 0;
                dwFlags = DWM_BB.Enable;
            }

            public System.Drawing.Region Region
            {
                get { return System.Drawing.Region.FromHrgn(hRgnBlur); }
            }

            public bool TransitionOnMaximized
            {
                get { return fTransitionOnMaximized > 0; }
                set
                {
                    fTransitionOnMaximized = value ? 1 : 0;
                    dwFlags |= DWM_BB.TransitionMaximized;
                }
            }

            public void SetRegion(System.Drawing.Graphics graphics, System.Drawing.Region region)
            {
                hRgnBlur = region.GetHrgn(graphics);
                dwFlags |= DWM_BB.BlurRegion;
            }
        }

        [DllImport("dwmapi.dll")]
        static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);
        public static void EnableBlur(Window window)
        {
            if (SystemParameters.HighContrast)
            {
                return; // Blur is not useful in high contrast mode 
            }
            SetAccentPolicy(window, NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND);

            var blurBehindParameters = new DWM_BLURBEHIND();
            blurBehindParameters.dwFlags = DWM_BB.Enable;
            blurBehindParameters.fEnable = 1;
            blurBehindParameters.hRgnBlur = IntPtr.Zero;

            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            DwmEnableBlurBehindWindow(windowHandle, ref blurBehindParameters);
        }


        public static void DisableBlur(Window window)
        {
            SetAccentPolicy(window, NativeMethods.AccentState.ACCENT_DISABLED);

            var blurBehindParameters = new DWM_BLURBEHIND();
            blurBehindParameters.dwFlags = DWM_BB.Enable;
            blurBehindParameters.fEnable = 0;
            blurBehindParameters.hRgnBlur = IntPtr.Zero;

            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            DwmEnableBlurBehindWindow(windowHandle, ref blurBehindParameters);
        }

        private static void SetAccentPolicy(Window window, NativeMethods.AccentState accentState)
        {
            var windowHelper = new WindowInteropHelper(window);
            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = GetAccentFlagsForTaskbarPosition(),
                AnimationId = 2
            };
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);
            var data = new NativeMethods.WindowCompositionAttribData
            {
                Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        private static NativeMethods.AccentFlags GetAccentFlagsForTaskbarPosition()
        {
            return NativeMethods.AccentFlags.DrawAllBorders;
        }
    }
}
