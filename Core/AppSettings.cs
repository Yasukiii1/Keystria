using System.Collections.Generic;

namespace KeyboardControl.Core;

public sealed class AppSettings
{
    public int MouseSpeed { get; set; } = 8;

    public List<KeyBinding> Bindings { get; set; } = new();
}