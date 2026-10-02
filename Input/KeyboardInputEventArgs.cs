using System;
using KeyboardControl.Core;

namespace KeyboardControl.Input;

public sealed class KeyboardInputEventArgs : EventArgs
{
    public uint VirtualKeyCode { get; }

    public bool IsKeyDown { get; }

    public bool IsKeyUp { get; }

    public bool Control { get; }

    public bool Shift { get; }

    public bool Alt { get; }

    public ControlModifiers Modifiers { get; }

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

        ControlModifiers modifiers = ControlModifiers.None;

        if (control)
            modifiers |= ControlModifiers.Control;

        if (shift)
            modifiers |= ControlModifiers.Shift;

        if (alt)
            modifiers |= ControlModifiers.Alt;

        Modifiers = modifiers;
    }
}
