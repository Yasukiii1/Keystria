using System;
using KeyboardControl.Commands;
using KeyboardControl.Input;

namespace KeyboardControl.Services;

public sealed class ControlService : IDisposable
{
    public KeyboardInputEngine Keyboard { get; } = new();
    public CommandEngine Commands { get; } = new();

    public bool IsEnabled { get; private set; }

    public event EventHandler<bool>? EnabledChanged;

    public ControlService()
    {
        Keyboard.KeyEvent += OnKeyEvent;

        Commands.Register(
            ControlCommand.ToggleControl,
            ToggleControl);
    }

    public void Start()
    {
        Keyboard.Start();
    }

    public void Stop()
    {
        Keyboard.Stop();
    }

    private void OnKeyEvent(
        object? sender,
        KeyboardInputEventArgs e)
    {
        if (!e.IsKeyDown)
            return;

        const uint VK_X = 0x58;

        if (e.VirtualKeyCode == VK_X && e.Control)
        {
            Commands.Execute(ControlCommand.ToggleControl);
        }
    }

    private void ToggleControl()
    {
        IsEnabled = !IsEnabled;
        EnabledChanged?.Invoke(this, IsEnabled);
    }

    public void Dispose()
    {
        Keyboard.KeyEvent -= OnKeyEvent;
        Keyboard.Dispose();
        GC.SuppressFinalize(this);
    }
}