using System;

namespace KeyboardControl.Core;

[Flags]
public enum ControlModifiers
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4
}