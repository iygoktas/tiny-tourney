using System;
using System.Text.Json;
using Godot;

namespace TinyTourney.Localization;

public partial class LocalizationManager : Node
{
	private const string SettingsPath = "user://settings.json";

	public static LocalizationManager Instance { get; private set; }
	public static readonly string[] SupportedLanguages = { "en", "tr" };

	public string CurrentLanguage { get; private set; } = "en";

	public override void _Ready()
	{
		Instance = this;

		string language = "en";
		if (FileAccess.FileExists(SettingsPath))
		{
			using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Read);
			string json = file.GetAsText();
			var settings = JsonSerializer.Deserialize<LocalizationSettings>(json);
			if (settings != null)
			{
				language = settings.Language;
			}
		}

		SetLanguage(language);
	}

	public void SetLanguage(string languageCode)
	{
		if (Array.IndexOf(SupportedLanguages, languageCode) < 0)
		{
			return;
		}

		TranslationServer.SetLocale(languageCode);
		CurrentLanguage = languageCode;

		var settings = new LocalizationSettings { Language = languageCode };
		string json = JsonSerializer.Serialize(settings);
		using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	public void RunSelfTest()
	{
		SetLanguage("en");
		GD.Print($"[en] BTN_FIGHT = {TranslationServer.Translate("BTN_FIGHT")}");
		GD.Print($"[en] STAT_STR = {TranslationServer.Translate("STAT_STR")}");

		SetLanguage("tr");
		GD.Print($"[tr] BTN_FIGHT = {TranslationServer.Translate("BTN_FIGHT")}");
		GD.Print($"[tr] STAT_STR = {TranslationServer.Translate("STAT_STR")}");

		SetLanguage("en");
	}
}
