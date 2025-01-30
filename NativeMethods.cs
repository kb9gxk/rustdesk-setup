using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RustdeskSetup
{
    internal static class NativeMethods
    {
        // Declaration for MessageBox function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        internal static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Add other DllImport declarations here for other native functions as needed
        internal const int SW_HIDE = 0;
        internal const int SW_SHOW = 5;

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
