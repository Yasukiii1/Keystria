using System;
using System.Windows;
using KeyboardControl.Services;

namespace KeyboardControl;

public partial class MainWindow : Window
{
    private readonly ControlService _controlService;

    public MainWindow()
    {
        InitializeComponent();

        _controlService = new ControlService();
        _controlService.EnabledChanged += OnEnabledChanged;

        Loaded += OnLoaded;
        Closed += OnClosed;

        UpdateWindowState(false);
    }

    private void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        _controlService.Start();
    }

    private void OnClosed(
        object sender,
        EventArgs e)
    {
        _controlService.Dispose();
    }

    private void OnEnabledChanged(
        object sender,
        bool enabled)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateWindowState(enabled);
        });
    }

    private void UpdateWindowState(bool enabled)
    {
        Title = enabled
            ? "Keyboard Control — ON"
            : "Keyboard Control — OFF";
    }
}