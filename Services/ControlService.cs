using System;
using System.Threading;
using KeyboardControl.Commands;
using KeyboardControl.Input;
using KeyboardControl.Interaction;

namespace KeyboardControl.Services;

public sealed class ControlService : IDisposable
{
    public KeyboardInputEngine Keyboard { get; } = new();
    public CommandEngine Commands { get; } = new();
    public MouseInteractionEngine Mouse { get; } = new();

    public bool IsEnabled { get; private set; }

    public event EventHandler<bool> EnabledChanged;

    private Timer _movementTimer;

    private bool _moveUp;
    private bool _moveDown;
    private bool _moveLeft;
    private bool _moveRight;

    public ControlService()
    {
        Keyboard.KeyEvent += OnKeyEvent;
        Keyboard.ShouldConsumeKey = ShouldConsumeKey;

        Commands.Register(
            ControlCommand.ToggleControl,
            ToggleControl);

        Commands.Register(
            ControlCommand.MoveUp,
            Mouse.MoveUp);

        Commands.Register(
            ControlCommand.MoveDown,
            Mouse.MoveDown);

        Commands.Register(
            ControlCommand.MoveLeft,
            Mouse.MoveLeft);

        Commands.Register(
            ControlCommand.MoveRight,
            Mouse.MoveRight);
    }

    public void Start()
    {
        Keyboard.Start();
    }

    public void Stop()
    {
        Keyboard.Stop();
        StopMovementTimer();
        ResetMovementState();
    }

    private void OnKeyEvent(
        object sender,
        KeyboardInputEventArgs e)
    {
        switch (e.VirtualKeyCode)
        {
            case 0x57: // W
            case 0x26: // Up Arrow
                _moveUp = e.IsKeyDown;
                break;

            case 0x53: // S
            case 0x28: // Down Arrow
                _moveDown = e.IsKeyDown;
                break;

            case 0x41: // A
            case 0x25: // Left Arrow
                _moveLeft = e.IsKeyDown;
                break;

            case 0x44: // D
            case 0x27: // Right Arrow
                _moveRight = e.IsKeyDown;
                break;
        }

        if (e.IsKeyDown &&
            e.VirtualKeyCode == 0x58 &&
            e.Control)
        {
            Commands.Execute(ControlCommand.ToggleControl);
        }
    }

    private bool ShouldConsumeKey(KeyboardInputEventArgs e)
    {
        if (e.IsKeyDown &&
            e.VirtualKeyCode == 0x58 &&
            e.Control)
        {
            return true;
        }

        if (!IsEnabled)
            return false;

        return e.VirtualKeyCode == 0x57 ||
               e.VirtualKeyCode == 0x53 ||
               e.VirtualKeyCode == 0x41 ||
               e.VirtualKeyCode == 0x44 ||
               e.VirtualKeyCode == 0x25 ||
               e.VirtualKeyCode == 0x26 ||
               e.VirtualKeyCode == 0x27 ||
               e.VirtualKeyCode == 0x28;
    }

    private void ToggleControl()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled)
        {
            StartMovementTimer();
        }
        else
        {
            StopMovementTimer();
            ResetMovementState();
        }

        EnabledChanged?.Invoke(this, IsEnabled);
    }

    private void StartMovementTimer()
    {
        StopMovementTimer();

        _movementTimer = new Timer(
            OnMovementTick,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(16));
    }

    private void StopMovementTimer()
    {
        _movementTimer?.Dispose();
        _movementTimer = null;
    }

    private void OnMovementTick(object state)
    {
        if (!IsEnabled)
            return;

        int horizontal = 0;
        int vertical = 0;

        if (_moveLeft)
            horizontal--;

        if (_moveRight)
            horizontal++;

        if (_moveUp)
            vertical--;

        if (_moveDown)
            vertical++;

        if (horizontal != 0)
            Commands.Execute(
                horizontal < 0
                    ? ControlCommand.MoveLeft
                    : ControlCommand.MoveRight);

        if (vertical != 0)
            Commands.Execute(
                vertical < 0
                    ? ControlCommand.MoveUp
                    : ControlCommand.MoveDown);
    }

    private void ResetMovementState()
    {
        _moveUp = false;
        _moveDown = false;
        _moveLeft = false;
        _moveRight = false;
    }

    public void Dispose()
    {
        Stop();

        Keyboard.KeyEvent -= OnKeyEvent;
        Keyboard.Dispose();

        GC.SuppressFinalize(this);
    }
}