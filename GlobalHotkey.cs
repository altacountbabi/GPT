using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GPT
{
    public class GlobalHotkey
    {
        private readonly int _id;
        private readonly IntPtr _handle;
        private readonly uint _modifiers;
        private readonly uint _key;
        private readonly Action _action;

        public GlobalHotkey(ModifierKeys modifierKeys, Key key, Action action, Window window)
        {
            _modifiers = (uint)modifierKeys;
            _key = (uint)KeyInterop.VirtualKeyFromKey(key);
            _id = GetHashCode();
            _handle = new WindowInteropHelper(window).Handle;
            _action = action;

            if (!RegisterHotkey())
            {
                throw new Exception("Could not register keybind");
            }

            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        private bool RegisterHotkey()
        {
            return NativeMethods.RegisterHotKey(_handle, _id, _modifiers, _key);
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (handled || msg.message != NativeMethods.WM_HOTKEY || (int)msg.wParam != _id) return;

            _action.Invoke();
            handled = true;
        }

        public void UnregisterHotkey()
        {
            NativeMethods.UnregisterHotKey(_handle, _id);
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
        }

        ~GlobalHotkey()
        {
            UnregisterHotkey();
        }

        private static class NativeMethods
        {
            public const int WM_HOTKEY = 0x0312;

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }
    }
}
