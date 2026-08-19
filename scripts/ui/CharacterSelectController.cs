using Godot;
using TinyTourney.Core;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class CharacterSelectController : Control
{
	[Export] public ItemList SlotList;
	[Export] public Button ContinueButton;
	[Export] public Button NewCharacterButton;
	[Export] public Button DeleteButton;
	[Export] public ConfirmationDialog DeleteConfirmDialog;

	private int _selectedSlot = -1;
	private bool _selectedSlotUsed;

	public override void _Ready()
	{
		SlotList.ItemSelected += OnSlotSelected;
		ContinueButton.Pressed += OnContinuePressed;
		NewCharacterButton.Pressed += OnNewCharacterPressed;
		DeleteButton.Pressed += OnDeletePressed;
		DeleteConfirmDialog.Confirmed += OnDeleteConfirmed;

		RefreshSlotList();
	}

	private void RefreshSlotList()
	{
		SlotList.Clear();

		for (int i = 0; i < SaveManager.MaxSlots; i++)
		{
			if (SaveManager.SlotExists(i))
			{
				var save = SaveManager.LoadSlot(i);
				var race = ContentRepository.GetRaceById(save.RaceId);
				string raceLabel = race != null ? TranslationServer.Translate($"RACE_{race.Id.ToUpper()}") : save.RaceId;
				SlotList.AddItem($"{save.CharacterName} — Lv.{save.Level} {raceLabel}");
			}
			else
			{
				SlotList.AddItem(TranslationServer.Translate("SLOT_EMPTY"));
			}

			SlotList.SetItemMetadata(SlotList.ItemCount - 1, i);
		}

		_selectedSlot = -1;
		_selectedSlotUsed = false;
		ContinueButton.Disabled = true;
		DeleteButton.Disabled = true;
		NewCharacterButton.Disabled = SaveManager.ListUsedSlots().Count >= SaveManager.MaxSlots;
	}

	private void OnSlotSelected(long index)
	{
		_selectedSlot = (int)SlotList.GetItemMetadata((int)index);
		_selectedSlotUsed = SaveManager.SlotExists(_selectedSlot);

		ContinueButton.Disabled = !_selectedSlotUsed;
		DeleteButton.Disabled = !_selectedSlotUsed;
	}

	private void OnContinuePressed()
	{
		if (!_selectedSlotUsed)
		{
			return;
		}

		GameState.Instance.LoadSlot(_selectedSlot);
		GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
	}

	private void OnNewCharacterPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/screens/character_create_controller.tscn");
	}

	private void OnDeletePressed()
	{
		if (!_selectedSlotUsed)
		{
			return;
		}

		DeleteConfirmDialog.PopupCentered();
	}

	private void OnDeleteConfirmed()
	{
		if (!_selectedSlotUsed)
		{
			return;
		}

		SaveManager.DeleteSlot(_selectedSlot);
		RefreshSlotList();
	}
}
