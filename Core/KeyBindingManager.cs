using System.Collections.Generic;
using KeyboardControl.Commands;

namespace KeyboardControl.Core;

public sealed class KeyBindingManager
{
    private readonly List<KeyBinding> _bindings = new();

    public IReadOnlyList<KeyBinding> Bindings => _bindings;

    public KeyBindingManager()
    {
        SetDefaults();
    }

    public void SetDefaults()
    {
        _bindings.Clear();

        _bindings.Add(new KeyBinding(0x57, ControlCommand.MoveUp));        // W
        _bindings.Add(new KeyBinding(0x53, ControlCommand.MoveDown));      // S
        _bindings.Add(new KeyBinding(0x41, ControlCommand.MoveLeft));      // A
        _bindings.Add(new KeyBinding(0x44, ControlCommand.MoveRight));     // D

        _bindings.Add(new KeyBinding(0x26, ControlCommand.MoveUp));        // Up
        _bindings.Add(new KeyBinding(0x28, ControlCommand.MoveDown));      // Down
        _bindings.Add(new KeyBinding(0x25, ControlCommand.MoveLeft));      // Left
        _bindings.Add(new KeyBinding(0x27, ControlCommand.MoveRight));     // Right

        _bindings.Add(new KeyBinding(0xDB, ControlCommand.LeftClick));     // [
        _bindings.Add(new KeyBinding(0xDD, ControlCommand.RightClick));    // ]
    }

    public bool TryGetCommand(uint virtualKeyCode, out ControlCommand command)
    {
        foreach (KeyBinding binding in _bindings)
        {
            if (binding.VirtualKeyCode == virtualKeyCode)
            {
                command = binding.Command;
                return true;
            }
        }

        command = default;
        return false;
    }
}