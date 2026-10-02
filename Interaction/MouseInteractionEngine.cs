using System;
using System.Runtime.InteropServices;

namespace KeyboardControl.Interaction;

public sealed class MouseInteractionEngine
{
    public int Speed { get; set; } = 8;

    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    public void Move(int horizontal, int vertical)
    {
        if (horizontal == 0 && vertical == 0)
            return;

        if (!GetCursorPos(out POINT position))
            return;

        int newX = position.X + horizontal * Speed;
        int newY = position.Y + vertical * Speed;

        int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        int right = left + width - 1;
        int bottom = top + height - 1;

        newX = Math.Clamp(newX, left, right);
        newY = Math.Clamp(newY, top, bottom);

        SetCursorPos(newX, newY);
    }

    public void MoveUp()
    {
        Move(0, -1);
    }

    public void MoveDown()
    {
        Move(0, 1);
    }

    public void MoveLeft()
    {
        Move(-1, 0);
    }

    public void MoveRight()
    {
        Move(1, 0);
    }

    public void LeftButtonDown()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
    }

    public void LeftButtonUp()
    {
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }

    public void RightButtonDown()
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
    }

    public void RightButtonUp()
    {
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
    }

    public void ReleaseAllButtons()
    {
        LeftButtonUp();
        RightButtonUp();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern void mouse_event(
        uint dwFlags,
        uint dx,
        uint dy,
        uint dwData,
        UIntPtr dwExtraInfo);
}