using Godot;
using TinyTourney.Core;
using TinyTourney.Localization;

namespace TinyTourney.UI;

public partial class SettingsController : Control
{
	[Export] public CheckButton MusicCheckButton;
	[Export] public CheckButton SfxCheckButton;
	[Export] public OptionButton LanguageOptionButton;
	[Export] public Button BackButton;

	/// <summary>
	/// Returns to Character Select so the player can switch which save they're playing.
	/// Optional: leave it unassigned and Settings simply has no such path, same convention
	/// as the other screens' optional exports.
	/// </summary>
	[Export] public Button CharacterSelectButton;

	public override void _Ready()
	{
		MusicCheckButton.Text = TranslationServer.Translate("SETTINGS_MUSIC");
		SfxCheckButton.Text = TranslationServer.Translate("SETTINGS_SFX");
		BackButton.Text = TranslationServer.Translate("BTN_BACK");

		if (CharacterSelectButton != null)
		{
			CharacterSelectButton.Text = TranslationServer.Translate("BTN_CHARACTER_SELECT");
			CharacterSelectButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/screens/character_select.tscn");
		}

		MusicCheckButton.ButtonPressed = AudioSettingsManager.Instance.MusicEnabled;
		SfxCheckButton.ButtonPressed = AudioSettingsManager.Instance.SfxEnabled;
		MusicCheckButton.Toggled += enabled => AudioSettingsManager.Instance.SetMusicEnabled(enabled);
		SfxCheckButton.Toggled += enabled => AudioSettingsManager.Instance.SetSfxEnabled(enabled);

		LanguageOptionButton.Clear();
		for (int i = 0; i < LocalizationManager.SupportedLanguages.Length; i++)
		{
			string code = LocalizationManager.SupportedLanguages[i];
			string label = TranslationServer.Translate($"LANG_{code.ToUpper()}");
			LanguageOptionButton.AddItem(label);
			LanguageOptionButton.SetItemMetadata(i, code);

			if (code == LocalizationManager.Instance.CurrentLanguage)
			{
				LanguageOptionButton.Selected = i;
			}
		}
		LanguageOptionButton.ItemSelected += OnLanguageSelected;

		BackButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
	}

	private void OnLanguageSelected(long index)
	{
		string code = (string)LanguageOptionButton.GetItemMetadata((int)index);
		LocalizationManager.Instance.SetLanguage(code);
	}
}
