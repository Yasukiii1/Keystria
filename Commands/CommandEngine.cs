using System;
using System.Collections.Generic;

namespace KeyboardControl.Commands;

public sealed class CommandEngine
{
    private readonly Dictionary<ControlCommand, Action> _commands = new();

    public void Register(ControlCommand command, Action action)
    {
        _commands[command] = action;
    }

    public bool Execute(ControlCommand command)
    {
        if (!_commands.TryGetValue(command, out Action action))
            return false;

        action();
        return true;
    }
}