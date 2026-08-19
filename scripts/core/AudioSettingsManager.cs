using System.Text.Json;
using Godot;

namespace TinyTourney.Core;

public partial class AudioSettingsManager : Node
{
    private const string SettingsPath = "user://audio_settings.json";

    public static AudioSettingsManager Instance { get; private set; }

    public bool MusicEnabled { get; private set; } = true;
    public bool SfxEnabled { get; private set; } = true;

    public override void _Ready()
    {
        Instance = this;

        if (FileAccess.FileExists(SettingsPath))
        {
            using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            var settings = JsonSerializer.Deserialize<AudioSettings>(json);
            if (settings != null)
            {
                MusicEnabled = settings.MusicEnabled;
                SfxEnabled = settings.SfxEnabled;
            }
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        Save();
    }

    public void SetSfxEnabled(bool enabled)
    {
        SfxEnabled = enabled;
        Save();
    }

    private void Save()
    {
        var settings = new AudioSettings { MusicEnabled = MusicEnabled, SfxEnabled = SfxEnabled };
        string json = JsonSerializer.Serialize(settings);
        using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }
}
