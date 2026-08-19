using Godot;
using TinyTourney.Core;
using TinyTourney.Localization;

namespace TinyTourney.UI;

public partial class SettingsController : Control
{
	[Export] public CheckButton MusicCheckButton;
	[Export] public CheckButton SfxCheckButton;
	[Export] public OptionButton LanguageOptionButton;
	[Export] public ItemList SlotList;
	[Export] public Button DeleteSlotButton;
	[Export] public ConfirmationDialog DeleteConfirmDialog;
	[Export] public Button BackButton;

	private int _selectedSlot = -1;

	public override void _Ready()
	{
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

		SlotList.ItemSelected += OnSlotSelected;
		DeleteSlotButton.Pressed += OnDeleteSlotPressed;
		DeleteConfirmDialog.Confirmed += OnDeleteConfirmed;

		BackButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");

		RefreshSlotList();
	}

	private void OnLanguageSelected(long index)
	{
		string code = (string)LanguageOptionButton.GetItemMetadata((int)index);
		LocalizationManager.Instance.SetLanguage(code);
	}

	private void RefreshSlotList()
	{
		SlotList.Clear();

		foreach (int slotIndex in SaveManager.ListUsedSlots())
		{
			var save = SaveManager.LoadSlot(slotIndex);
			SlotList.AddItem($"{save.CharacterName} — Lv.{save.Level}");
			SlotList.SetItemMetadata(SlotList.ItemCount - 1, slotIndex);
		}

		_selectedSlot = -1;
		DeleteSlotButton.Disabled = true;
	}

	private void OnSlotSelected(long index)
	{
		_selectedSlot = (int)SlotList.GetItemMetadata((int)index);
		DeleteSlotButton.Disabled = false;
	}

	private void OnDeleteSlotPressed()
	{
		if (_selectedSlot < 0)
		{
			return;
		}

		DeleteConfirmDialog.PopupCentered();
	}

	private void OnDeleteConfirmed()
	{
		if (_selectedSlot < 0)
		{
			return;
		}

		SaveManager.DeleteSlot(_selectedSlot);
		RefreshSlotList();
	}
}
