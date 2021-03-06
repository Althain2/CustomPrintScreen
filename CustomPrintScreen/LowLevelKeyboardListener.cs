﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace CustomPrintScreen
{
    public class LowLevelKeyboardListener
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<EventArgs> OnKeyPressed;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public LowLevelKeyboardListener()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);
                Trace.WriteLine(keyPressed);

                if (keyPressed == Key.PrintScreen && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (Handler.Bitmaps.Count > 0 && Handler.AdvancedMode == false)
                    {
                        Handler.ClearData();
                        (Handler.mainWindow as MainWindow).imgs.Children.Clear();
                    }

                    Handler.AdvancedMode = true;

                    OnKeyPressed?.Invoke(this, new EventArgs());
                }
                else if (keyPressed == Key.PrintScreen)
                {
                    if (Handler.Bitmaps.Count > 0 && Handler.AdvancedMode == true)
                    {
                        Handler.ClearData();
                        (Handler.mainWindow as MainWindow).imgs.Children.Clear();
                    }

                    Handler.AdvancedMode = false;
                    OnKeyPressed?.Invoke(this, new EventArgs());
                }
                else if (keyPressed == Key.Escape)
                {
                    if (Application.Current.MainWindow.Visibility == Visibility.Visible)
                    {
                        (App.Current.MainWindow as MainWindow).CloseWindow();
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}