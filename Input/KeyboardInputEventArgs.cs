using System;

namespace KeyboardControl.Input;

public sealed class KeyboardInputEventArgs : EventArgs
{
    public uint VirtualKeyCode { get; }
    public bool IsKeyDown { get; }
    public bool IsKeyUp { get; }
    public bool Control { get; }
    public bool Shift { get; }
    public bool Alt { get; }

    public KeyboardInputEventArgs(
        uint virtualKeyCode,
        bool isKeyDown,
        bool isKeyUp,
        bool control,
        bool shift,
        bool alt)
    {
        VirtualKeyCode = virtualKeyCode;
        IsKeyDown = isKeyDown;
        IsKeyUp = isKeyUp;
        Control = control;
        Shift = shift;
        Alt = alt;
    }
}