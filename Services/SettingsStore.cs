using System;
using System.IO;
using System.Text.Json;
using KeyboardControl.Core;

namespace KeyboardControl.Services;

public sealed class SettingsStore
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFile;

    public SettingsStore()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "KeyboardControl");

        _settingsFile = Path.Combine(
            _settingsDirectory,
            "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsFile))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(_settingsFile);

            AppSettings settings =
                JsonSerializer.Deserialize<AppSettings>(json);

            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_settingsDirectory);

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json =
            JsonSerializer.Serialize(settings, options);

        File.WriteAllText(
            _settingsFile,
            json);
    }
}