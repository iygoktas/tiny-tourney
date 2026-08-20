using Godot;
using TinyTourney.Core;
using TinyTourney.Data;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class CharacterCreateController : Control
{
	[Export] public OptionButton RaceOptionButton;
	[Export] public LineEdit NameLineEdit;
	[Export] public Button ConfirmButton;

	// Optional — leave unassigned in the scene and these simply won't update.
	// Lets this controller work before the preview/description nodes exist yet.
	[Export] public TextureRect RacePreview;
	[Export] public Label RaceDescriptionLabel;
	[Export] public Label NameWarningLabel;

	public override void _Ready()
	{
		RaceOptionButton.Clear();
		foreach (var race in ContentRepository.AllRaces)
		{
			string label = TranslationServer.Translate($"RACE_{race.Id.ToUpper()}");
			RaceOptionButton.AddItem(label);
			RaceOptionButton.SetItemMetadata(RaceOptionButton.ItemCount - 1, race.Id);
		}

		ConfirmButton.Text = TranslationServer.Translate("BTN_CONFIRM");
		ConfirmButton.Pressed += OnConfirmPressed;

		RaceOptionButton.ItemSelected += OnRaceSelected;
		if (NameWarningLabel != null)
		{
			NameWarningLabel.Text = string.Empty;
		}

		if (RaceOptionButton.ItemCount > 0)
		{
			RaceOptionButton.Selected = 0;
			OnRaceSelected(0);
		}
	}

	private void OnRaceSelected(long index)
	{
		string raceId = (string)RaceOptionButton.GetItemMetadata((int)index);
		var race = ContentRepository.GetRaceById(raceId);
		ShowRacePreview(race);

		if (NameWarningLabel != null)
		{
			NameWarningLabel.Text = string.Empty;
		}
	}

	private void ShowRacePreview(RaceData race)
	{
		if (RacePreview != null)
		{
			RacePreview.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
			if (race?.ReferenceImagePath is { Length: > 0 } path && ResourceLoader.Exists(path))
			{
				RacePreview.Texture = GD.Load<Texture2D>(path);
			}
			else
			{
				RacePreview.Texture = null;
			}
		}

		if (RaceDescriptionLabel != null)
		{
			RaceDescriptionLabel.Text = race != null
				? TranslationServer.Translate($"RACE_{race.Id.ToUpper()}_DESC")
				: string.Empty;
		}
	}

	private void OnConfirmPressed()
	{
		string characterName = NameLineEdit.Text.Trim();
		if (string.IsNullOrEmpty(characterName) || RaceOptionButton.Selected < 0)
		{
			if (NameWarningLabel != null && string.IsNullOrEmpty(characterName))
			{
				NameWarningLabel.Text = TranslationServer.Translate("WARN_NAME_REQUIRED");
			}
			return;
		}

		int slotIndex = -1;
		for (int i = 0; i < SaveManager.MaxSlots; i++)
		{
			if (!SaveManager.SlotExists(i))
			{
				slotIndex = i;
				break;
			}
		}

		if (slotIndex < 0)
		{
			return;
		}

		string raceId = (string)RaceOptionButton.GetItemMetadata(RaceOptionButton.Selected);
		GameState.Instance.NewCharacter(slotIndex, raceId, characterName);
		GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
	}
}
