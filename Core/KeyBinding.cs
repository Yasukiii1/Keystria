using KeyboardControl.Commands;

namespace KeyboardControl.Core;

public sealed class KeyBinding
{
    public uint VirtualKeyCode { get; }
    public ControlCommand Command { get; }

    public KeyBinding(uint virtualKeyCode, ControlCommand command)
    {
        VirtualKeyCode = virtualKeyCode;
        Command = command;
    }
}