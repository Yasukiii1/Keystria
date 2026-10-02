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

        // Activation
        _bindings.Add(new KeyBinding(
            0x58,
            ControlModifiers.Control,
            ControlCommand.ToggleControl));

        // WASD
        _bindings.Add(new KeyBinding(
            0x57,
            ControlModifiers.None,
            ControlCommand.MoveUp));

        _bindings.Add(new KeyBinding(
            0x53,
            ControlModifiers.None,
            ControlCommand.MoveDown));

        _bindings.Add(new KeyBinding(
            0x41,
            ControlModifiers.None,
            ControlCommand.MoveLeft));

        _bindings.Add(new KeyBinding(
            0x44,
            ControlModifiers.None,
            ControlCommand.MoveRight));

        // Arrow keys
        _bindings.Add(new KeyBinding(
            0x26,
            ControlModifiers.None,
            ControlCommand.MoveUp));

        _bindings.Add(new KeyBinding(
            0x28,
            ControlModifiers.None,
            ControlCommand.MoveDown));

        _bindings.Add(new KeyBinding(
            0x25,
            ControlModifiers.None,
            ControlCommand.MoveLeft));

        _bindings.Add(new KeyBinding(
            0x27,
            ControlModifiers.None,
            ControlCommand.MoveRight));

        // Mouse buttons
        _bindings.Add(new KeyBinding(
            0x51,
            ControlModifiers.None,
            ControlCommand.LeftClick));   // Q

        _bindings.Add(new KeyBinding(
            0x45,
            ControlModifiers.None,
            ControlCommand.RightClick));  // E
    }

    public void Load(IEnumerable<KeyBinding> savedBindings)
    {
        _bindings.Clear();

        foreach (KeyBinding binding in savedBindings)
        {
            bool conflict = false;

            foreach (KeyBinding existing in _bindings)
            {
                if (existing.VirtualKeyCode == binding.VirtualKeyCode &&
                    existing.Command != binding.Command)
                {
                    conflict = true;
                    break;
                }
            }

            if (!conflict)
            {
                _bindings.Add(binding);
            }
        }

        AddMissingDefaultCommands();
    }

    private void AddMissingDefaultCommands()
    {
        KeyBindingManager defaults = new();

        foreach (KeyBinding defaultBinding in defaults.Bindings)
        {
            bool commandExists = false;

            foreach (KeyBinding binding in _bindings)
            {
                if (binding.Command == defaultBinding.Command)
                {
                    commandExists = true;
                    break;
                }
            }

            if (!commandExists)
            {
                _bindings.Add(new KeyBinding(
                    defaultBinding.VirtualKeyCode,
                    defaultBinding.Modifiers,
                    defaultBinding.Command));
            }
        }
    }

    public bool TryGetCommand(
        uint virtualKeyCode,
        ControlModifiers modifiers,
        out ControlCommand command)
    {
        foreach (KeyBinding binding in _bindings)
        {
            if (binding.VirtualKeyCode == virtualKeyCode &&
                binding.Modifiers == modifiers)
            {
                command = binding.Command;
                return true;
            }
        }

        command = default;
        return false;
    }

    public bool TryGetCommandByKey(
        uint virtualKeyCode,
        out ControlCommand command)
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

    public bool TrySetBinding(
        ControlCommand command,
        uint virtualKeyCode,
        ControlModifiers modifiers,
        out ControlCommand conflictCommand)
    {
        conflictCommand = default;

        foreach (KeyBinding binding in _bindings)
        {
            if (binding.Command != command &&
                binding.VirtualKeyCode == virtualKeyCode)
            {
                conflictCommand = binding.Command;
                return false;
            }
        }

        _bindings.RemoveAll(
            binding => binding.Command == command);

        _bindings.Add(new KeyBinding(
            virtualKeyCode,
            modifiers,
            command));

        return true;
    }

    public List<KeyBinding> GetBindings(
        ControlCommand command)
    {
        List<KeyBinding> result = new();

        foreach (KeyBinding binding in _bindings)
        {
            if (binding.Command == command)
            {
                result.Add(binding);
            }
        }

        return result;
    }
}