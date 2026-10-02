using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using KeyboardControl.Commands;
using KeyboardControl.Core;
using KeyboardControl.Input;
using KeyboardControl.Services;

namespace KeyboardControl;

public partial class MainWindow : Window
{
    private readonly ControlService _controlService;
    private readonly Dictionary<ControlCommand, Button> _keybindButtons;

    public MainWindow()
    {
        InitializeComponent();

        _controlService = new ControlService();
        _controlService.EnabledChanged += OnEnabledChanged;

        Loaded += OnLoaded;
        Closed += OnClosed;

        _keybindButtons = new Dictionary<ControlCommand, Button>
        {
            [ControlCommand.ToggleControl] = ToggleControlButton,
            [ControlCommand.MoveUp] = MoveUpButton,
            [ControlCommand.MoveDown] = MoveDownButton,
            [ControlCommand.MoveLeft] = MoveLeftButton,
            [ControlCommand.MoveRight] = MoveRightButton,
            [ControlCommand.LeftClick] = LeftClickButton,
            [ControlCommand.RightClick] = RightClickButton
        };

        HomeView.Visibility = Visibility.Visible;
        SettingsView.Visibility = Visibility.Collapsed;

        UpdateWindowState(false);
        UpdateKeybindDisplay();

        MouseSpeedSlider.Value = _controlService.Mouse.Speed;
        MouseSpeedValueButton.Content =
            _controlService.Mouse.Speed.ToString();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _controlService.Start();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        _controlService.SaveSettings();
        _controlService.Dispose();
    }

    private void OnEnabledChanged(
        object sender,
        bool enabled)
    {
        Dispatcher.Invoke(() =>
            UpdateWindowState(enabled));
    }

    private void UpdateWindowState(bool enabled)
    {
        string version = GetAppVersion();

        Title = enabled
            ? $"Keystria v{version} — ON"
            : $"Keystria v{version} — OFF";
    }

    private static string GetAppVersion()
    {
        Version? version = typeof(MainWindow)
            .Assembly
            .GetName()
            .Version;

        if (version == null)
            return "1.1";

        return $"{version.Major}.{version.Minor}";
    }

    private void SettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        HomeView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;

        UpdateKeybindDisplay();

        MouseSpeedSlider.Value =
            _controlService.Mouse.Speed;

        MouseSpeedValueButton.Content =
            _controlService.Mouse.Speed.ToString();
    }

    private void BackButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _controlService.EndKeyCapture();

        SettingsView.Visibility = Visibility.Collapsed;
        HomeView.Visibility = Visibility.Visible;
    }

    private void KeybindButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Tag is not string commandName)
            return;

        if (!Enum.TryParse(
                commandName,
                out ControlCommand command))
        {
            return;
        }

        button.Content = "Select keybind";

        _controlService.BeginKeyCapture(
            keyboardEvent =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (keyboardEvent.VirtualKeyCode == 0x1B)
                    {
                        _controlService.EndKeyCapture();
                        UpdateKeybindDisplay();
                        return;
                    }

                    if (_controlService.Bindings.TrySetBinding(
                        command,
                        keyboardEvent.VirtualKeyCode,
                        keyboardEvent.Modifiers,
                        out ControlCommand conflict))
                    {
                        _controlService.SaveSettings();
                        _controlService.EndKeyCapture();
                        UpdateKeybindDisplay();
                    }
                    else
                    {
                        _controlService.EndKeyCapture();
                        UpdateKeybindDisplay();

                        MessageBox.Show(
                            $"That key is already assigned to {GetCommandDisplayName(conflict)}.",
                            "Keybind conflict",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                });
            });
    }

    private void MouseSpeedSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (_controlService == null)
            return;

        int speed = (int)Math.Round(e.NewValue);

        MouseSpeedValueButton.Content =
            speed.ToString();

        if (IsLoaded &&
            Math.Abs(e.NewValue - e.OldValue) > 0.001)
        {
            AnimateMouseSpeedValue();
        }

        _controlService.Mouse.Speed = speed;
        _controlService.SaveSettings();
    }

    private void AnimateMouseSpeedValue()
    {
        if (MouseSpeedValueButton.RenderTransform
            is not ScaleTransform transform)
        {
            return;
        }

        DoubleAnimation scaleAnimation = new(
            1.0,
            1.08,
            TimeSpan.FromMilliseconds(80))
        {
            AutoReverse = true,
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        DoubleAnimation opacityAnimation = new(
            1.0,
            0.72,
            TimeSpan.FromMilliseconds(80))
        {
            AutoReverse = true,
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        transform.BeginAnimation(
            ScaleTransform.ScaleXProperty,
            scaleAnimation);

        transform.BeginAnimation(
            ScaleTransform.ScaleYProperty,
            scaleAnimation);

        MouseSpeedValueButton.BeginAnimation(
            UIElement.OpacityProperty,
            opacityAnimation);
    }

    private void RestoreDefaultsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Restore all keybinds and mouse settings to their default values?",
            "Restore Defaults",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _controlService.EndKeyCapture();
        _controlService.RestoreDefaults();

        MouseSpeedSlider.Value =
            _controlService.Mouse.Speed;

        MouseSpeedValueButton.Content =
            _controlService.Mouse.Speed.ToString();

        UpdateKeybindDisplay();
    }

    private void UpdateKeybindDisplay()
    {
        foreach (KeyValuePair<ControlCommand, Button> entry
                 in _keybindButtons)
        {
            List<KeyBinding> bindings =
                _controlService.Bindings.GetBindings(entry.Key);

            if (bindings.Count == 0)
            {
                entry.Value.Content = "Select keybind";
                continue;
            }

            entry.Value.Content = string.Join(
                " / ",
                bindings.Select(FormatBinding));
        }
    }

    private static string FormatBinding(
        KeyBinding binding)
    {
        List<string> parts = new();

        if (binding.Modifiers.HasFlag(
                ControlModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (binding.Modifiers.HasFlag(
                ControlModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (binding.Modifiers.HasFlag(
                ControlModifiers.Alt))
        {
            parts.Add("Alt");
        }

        parts.Add(
            GetKeyName(binding.VirtualKeyCode));

        return string.Join(
            " + ",
            parts);
    }

    private static string GetKeyName(
        uint virtualKey)
    {
        return virtualKey switch
        {
            0x08 => "Backspace",
            0x09 => "Tab",
            0x0D => "Enter",
            0x10 => "Shift",
            0x11 => "Ctrl",
            0x12 => "Alt",
            0x13 => "Pause",
            0x14 => "Caps Lock",
            0x1B => "Escape",
            0x20 => "Space Bar",
            0x21 => "Page Up",
            0x22 => "Page Down",
            0x23 => "End",
            0x24 => "Home",

            0x25 => "←",
            0x26 => "↑",
            0x27 => "→",
            0x28 => "↓",

            0x2D => "Insert",
            0x2E => "Delete",

            0x5B => "Left Windows",
            0x5C => "Right Windows",

            0x60 => "Num 0",
            0x61 => "Num 1",
            0x62 => "Num 2",
            0x63 => "Num 3",
            0x64 => "Num 4",
            0x65 => "Num 5",
            0x66 => "Num 6",
            0x67 => "Num 7",
            0x68 => "Num 8",
            0x69 => "Num 9",

            0xDB => "[",
            0xDC => "\\",
            0xDD => "]",
            0xDE => "'",

            >= 0x41 and <= 0x5A =>
                ((char)virtualKey).ToString(),

            >= 0x30 and <= 0x39 =>
                ((char)virtualKey).ToString(),

            _ => $"VK {virtualKey}"
        };
    }

    private static string GetCommandDisplayName(
        ControlCommand command)
    {
        return command switch
        {
            ControlCommand.ToggleControl =>
                "Enable / Disable",

            ControlCommand.MoveUp =>
                "Move Up",

            ControlCommand.MoveDown =>
                "Move Down",

            ControlCommand.MoveLeft =>
                "Move Left",

            ControlCommand.MoveRight =>
                "Move Right",

            ControlCommand.LeftClick =>
                "Left Click",

            ControlCommand.RightClick =>
                "Scroll Modifier",

            _ => command.ToString()
        };
    }
}
