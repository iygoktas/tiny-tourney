using Godot;
using TinyTourney.Core;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class CharacterCreateController : Control
{
	[Export] public OptionButton RaceOptionButton;
	[Export] public LineEdit NameLineEdit;
	[Export] public Button ConfirmButton;

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
	}

	private void OnConfirmPressed()
	{
		string characterName = NameLineEdit.Text.Trim();
		if (string.IsNullOrEmpty(characterName) || RaceOptionButton.Selected < 0)
		{
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
