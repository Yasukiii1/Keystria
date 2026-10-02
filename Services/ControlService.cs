using System;
using System.Threading;
using KeyboardControl.Commands;
using KeyboardControl.Core;
using KeyboardControl.Input;
using KeyboardControl.Interaction;

namespace KeyboardControl.Services;

public sealed class ControlService : IDisposable
{
    public KeyboardInputEngine Keyboard { get; } = new();
    public CommandEngine Commands { get; } = new();
    public MouseInteractionEngine Mouse { get; } = new();
    public KeyBindingManager Bindings { get; } = new();

    public bool IsEnabled { get; private set; }

    public event EventHandler<bool> EnabledChanged;

    private Timer _movementTimer;

    private bool _moveUp;
    private bool _moveDown;
    private bool _moveLeft;
    private bool _moveRight;

    private bool _leftButtonHeld;
    private bool _rightButtonHeld;

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

        Commands.Register(
            ControlCommand.LeftClick,
            Mouse.LeftButtonDown);

        Commands.Register(
            ControlCommand.RightClick,
            Mouse.RightButtonDown);
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
        ReleaseMouseButtons();
    }

    private void OnKeyEvent(
        object sender,
        KeyboardInputEventArgs e)
    {
        // Ctrl + X = global activation toggle.
        if (e.IsKeyDown &&
            e.VirtualKeyCode == 0x58 &&
            e.Control)
        {
            Commands.Execute(ControlCommand.ToggleControl);
            return;
        }

        if (!IsEnabled)
            return;

        if (!Bindings.TryGetCommand(
                e.VirtualKeyCode,
                out ControlCommand command))
        {
            return;
        }

        switch (command)
        {
            case ControlCommand.MoveUp:
                _moveUp = e.IsKeyDown;
                break;

            case ControlCommand.MoveDown:
                _moveDown = e.IsKeyDown;
                break;

            case ControlCommand.MoveLeft:
                _moveLeft = e.IsKeyDown;
                break;

            case ControlCommand.MoveRight:
                _moveRight = e.IsKeyDown;
                break;

            case ControlCommand.LeftClick:
                if (e.IsKeyDown && !_leftButtonHeld)
                {
                    _leftButtonHeld = true;
                    Mouse.LeftButtonDown();
                }
                else if (e.IsKeyUp && _leftButtonHeld)
                {
                    _leftButtonHeld = false;
                    Mouse.LeftButtonUp();
                }
                break;

            case ControlCommand.RightClick:
                if (e.IsKeyDown && !_rightButtonHeld)
                {
                    _rightButtonHeld = true;
                    Mouse.RightButtonDown();
                }
                else if (e.IsKeyUp && _rightButtonHeld)
                {
                    _rightButtonHeld = false;
                    Mouse.RightButtonUp();
                }
                break;
        }
    }

    private bool ShouldConsumeKey(
        KeyboardInputEventArgs e)
    {
        // Always consume Ctrl + X so it does not trigger
        // the normal Windows cut shortcut.
        if (e.IsKeyDown &&
            e.VirtualKeyCode == 0x58 &&
            e.Control)
        {
            return true;
        }

        if (!IsEnabled)
            return false;

        return Bindings.TryGetCommand(
            e.VirtualKeyCode,
            out _);
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
            ReleaseMouseButtons();
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
        {
            Commands.Execute(
                horizontal < 0
                    ? ControlCommand.MoveLeft
                    : ControlCommand.MoveRight);
        }

        if (vertical != 0)
        {
            Commands.Execute(
                vertical < 0
                    ? ControlCommand.MoveUp
                    : ControlCommand.MoveDown);
        }
    }

    private void ResetMovementState()
    {
        _moveUp = false;
        _moveDown = false;
        _moveLeft = false;
        _moveRight = false;
    }

    private void ReleaseMouseButtons()
    {
        if (_leftButtonHeld)
        {
            _leftButtonHeld = false;
            Mouse.LeftButtonUp();
        }

        if (_rightButtonHeld)
        {
            _rightButtonHeld = false;
            Mouse.RightButtonUp();
        }
    }

    public void Dispose()
    {
        Stop();

        Keyboard.KeyEvent -= OnKeyEvent;
        Keyboard.Dispose();

        GC.SuppressFinalize(this);
    }
}