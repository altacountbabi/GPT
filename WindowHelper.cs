using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GPT
{
    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static bool FocusWindow(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                IntPtr hWnd = processes[0].MainWindowHandle;
                return SetForegroundWindow(hWnd);
            }
            return false;
        }
    }
}