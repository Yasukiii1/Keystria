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

    public SettingsStore Settings { get; } = new();

    public bool IsEnabled { get; private set; }

    public event EventHandler<bool> EnabledChanged;

    private Timer _movementTimer;

    private bool _moveUp;
    private bool _moveDown;
    private bool _moveLeft;
    private bool _moveRight;

    private bool _leftButtonHeld;
    private bool _scrollModifierHeld;

    private DateTime _lastScrollTime = DateTime.MinValue;

    private Action<KeyboardInputEventArgs> _keyCaptureHandler;

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

        AppSettings settings = Settings.Load();

        Bindings.Load(settings.Bindings);
        Mouse.Speed = settings.MouseSpeed;
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

    public void BeginKeyCapture(
        Action<KeyboardInputEventArgs> handler)
    {
        _keyCaptureHandler = handler;
    }

    public void EndKeyCapture()
    {
        _keyCaptureHandler = null;
    }

    private void OnKeyEvent(
        object sender,
        KeyboardInputEventArgs e)
    {
        if (_keyCaptureHandler != null)
        {
            if (e.IsKeyDown)
            {
                _keyCaptureHandler(e);
            }

            return;
        }

        if (e.IsKeyDown &&
            Bindings.TryGetCommand(
                e.VirtualKeyCode,
                e.Modifiers,
                out ControlCommand activationCommand) &&
            activationCommand == ControlCommand.ToggleControl)
        {
            Commands.Execute(ControlCommand.ToggleControl);
            return;
        }

        if (!IsEnabled)
            return;

        if (!Bindings.TryGetCommand(
                e.VirtualKeyCode,
                e.Modifiers,
                out ControlCommand command))
        {
            if (!e.IsKeyUp ||
                !Bindings.TryGetCommandByKey(
                    e.VirtualKeyCode,
                    out command))
            {
                return;
            }
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
                _scrollModifierHeld = e.IsKeyDown;

                if (!e.IsKeyDown)
                {
                    _lastScrollTime = DateTime.MinValue;
                }

                break;
        }
    }

    private bool ShouldConsumeKey(
        KeyboardInputEventArgs e)
    {
        if (_keyCaptureHandler != null)
            return true;

        if (Bindings.TryGetCommand(
                e.VirtualKeyCode,
                e.Modifiers,
                out ControlCommand command) &&
            command == ControlCommand.ToggleControl)
        {
            return true;
        }

        if (!IsEnabled)
            return false;

        if (Bindings.TryGetCommand(
            e.VirtualKeyCode,
            e.Modifiers,
            out _))
        {
            return true;
        }

        if (e.IsKeyUp &&
            Bindings.TryGetCommandByKey(
                e.VirtualKeyCode,
                out _))
        {
            return true;
        }

        return false;
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

        EnabledChanged?.Invoke(
            this,
            IsEnabled);
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

        if (_scrollModifierHeld)
        {
            if (horizontal == 0 && vertical == 0)
                return;

            DateTime now = DateTime.UtcNow;

            if ((now - _lastScrollTime).TotalMilliseconds < 100)
                return;

            _lastScrollTime = now;

            if (vertical < 0)
                Mouse.ScrollUp();
            else if (vertical > 0)
                Mouse.ScrollDown();

            if (horizontal < 0)
                Mouse.ScrollLeft();
            else if (horizontal > 0)
                Mouse.ScrollRight();

            return;
        }

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
        _scrollModifierHeld = false;
        _lastScrollTime = DateTime.MinValue;
    }

    private void ReleaseMouseButtons()
    {
        if (_leftButtonHeld)
        {
            _leftButtonHeld = false;
            Mouse.LeftButtonUp();
        }

    }

    public void SaveSettings()
    {
        AppSettings settings = new()
        {
            MouseSpeed = Mouse.Speed,
            Bindings = new()
        };

        foreach (KeyBinding binding in Bindings.Bindings)
        {
            settings.Bindings.Add(
                new KeyBinding(
                    binding.VirtualKeyCode,
                    binding.Modifiers,
                    binding.Command));
        }

        Settings.Save(settings);
    }

    public void RestoreDefaults()
    {
        Bindings.SetDefaults();
        Mouse.Speed = 8;
        SaveSettings();
    }

    public void Dispose()
    {
        Stop();

        Keyboard.KeyEvent -= OnKeyEvent;
        Keyboard.Dispose();

        GC.SuppressFinalize(this);
    }
}