using System;
using System.Runtime.InteropServices;

namespace RustdeskSetup
{
    internal static class NativeMethods
    {
        // Declaration for MessageBox function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Add other DllImport declarations here for other native functions as needed
        internal const int SW_HIDE = 0;
        internal const int SW_SHOW = 5;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }



}
