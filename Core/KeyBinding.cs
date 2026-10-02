using KeyboardControl.Commands;

namespace KeyboardControl.Core;

public sealed class KeyBinding
{
    public uint VirtualKeyCode { get; set; }

    public ControlModifiers Modifiers { get; set; }

    public ControlCommand Command { get; set; }

    public KeyBinding()
    {
    }

    public KeyBinding(
        uint virtualKeyCode,
        ControlModifiers modifiers,
        ControlCommand command)
    {
        VirtualKeyCode = virtualKeyCode;
        Modifiers = modifiers;
        Command = command;
    }
}