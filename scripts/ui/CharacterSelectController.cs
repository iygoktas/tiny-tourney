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
		// Button captions live here rather than in the scene so they follow the chosen language.
		ContinueButton.Text = TranslationServer.Translate("BTN_CONTINUE");
		NewCharacterButton.Text = TranslationServer.Translate("BTN_NEW_CHARACTER");
		DeleteButton.Text = TranslationServer.Translate("BTN_DELETE");
		DeleteConfirmDialog.Title = TranslationServer.Translate("CONFIRM_DELETE_TITLE");
		DeleteConfirmDialog.DialogText = TranslationServer.Translate("CONFIRM_DELETE_TEXT");

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
				// An empty slot is a fact, not a choice — dim it so the real
				// characters carry the visual weight in the list.
				SlotList.SetItemCustomFgColor(SlotList.ItemCount - 1, new Color(0.55f, 0.51f, 0.44f, 0.55f));
			}

			SlotList.SetItemMetadata(SlotList.ItemCount - 1, i);
		}

		_selectedSlot = -1;
		_selectedSlotUsed = false;
		ContinueButton.Disabled = true;
		DeleteButton.Disabled = true;

		bool slotsFull = SaveManager.ListUsedSlots().Count >= SaveManager.MaxSlots;
		NewCharacterButton.Disabled = slotsFull;
		// Without this the button just sits there greyed out with no explanation.
		NewCharacterButton.TooltipText = slotsFull
			? TranslationServer.Translate("SLOTS_FULL_HINT")
			: string.Empty;

		// Land on the first character the player actually has, so Continue and Delete are
		// usable straight away instead of waiting on a selection in the list.
		SelectFirstUsedSlot();
	}

	private void SelectFirstUsedSlot()
	{
		for (int i = 0; i < SlotList.ItemCount; i++)
		{
			int slotIndex = (int)SlotList.GetItemMetadata(i);
			if (SaveManager.SlotExists(slotIndex))
			{
				SlotList.Select(i);
				OnSlotSelected(i);
				return;
			}
		}
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
