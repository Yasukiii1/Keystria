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
}