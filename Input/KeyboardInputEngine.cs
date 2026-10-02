using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardControl.Input;

public sealed class KeyboardInputEngine : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private const int VK_CONTROL = 0x11;
    private const int VK_SHIFT = 0x10;
    private const int VK_MENU = 0x12;

    private readonly LowLevelKeyboardProc _hookProc;
    private IntPtr _hookId = IntPtr.Zero;

    public event EventHandler<KeyboardInputEventArgs> KeyEvent;

    public KeyboardInputEngine()
    {
        _hookProc = HookCallback;
    }

    public void Start()
    {
        if (_hookId != IntPtr.Zero)
            return;

        using Process process = Process.GetCurrentProcess();
        using ProcessModule module = process.MainModule;

        IntPtr moduleHandle = GetModuleHandle(module.ModuleName);

        _hookId = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _hookProc,
            moduleHandle,
            0);

        if (_hookId == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to install keyboard hook. Windows error: {Marshal.GetLastWin32Error()}");
        }
    }

    public void Stop()
    {
        if (_hookId == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    private IntPtr HookCallback(
        int nCode,
        IntPtr wParam,
        IntPtr lParam)
    {
        if (nCode >= 0)
        {
            uint message = unchecked((uint)wParam.ToInt64());

            bool isKeyDown =
                message == WM_KEYDOWN ||
                message == WM_SYSKEYDOWN;

            bool isKeyUp =
                message == WM_KEYUP ||
                message == WM_SYSKEYUP;

            if (isKeyDown || isKeyUp)
            {
                KBDLLHOOKSTRUCT info =
                    Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                bool control = IsKeyPressed(VK_CONTROL);
                bool shift = IsKeyPressed(VK_SHIFT);
                bool alt = IsKeyPressed(VK_MENU);

                KeyEvent?.Invoke(
                    this,
                    new KeyboardInputEventArgs(
                        info.vkCode,
                        isKeyDown,
                        isKeyUp,
                        control,
                        shift,
                        alt));

                // Ctrl+X is reserved for Keyboard Control.
                if (isKeyDown &&
                    info.vkCode == 0x58 &&
                    control)
                {
                    return (IntPtr)1;
                }
            }
        }

        return CallNextHookEx(
            _hookId,
            nCode,
            wParam,
            lParam);
    }

    private static bool IsKeyPressed(int virtualKey)
    {
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(
        IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(
        int vKey);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(
        string lpModuleName);
}