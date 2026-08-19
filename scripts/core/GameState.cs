using System;
using Godot;
using TinyTourney.Data;

namespace TinyTourney.Core;

public partial class GameState : Node
{
	public static GameState Instance { get; private set; }

	public int ActiveSlotIndex { get; private set; } = -1;
	public SaveSlotData Active { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		RunSelfTest();
	}

	public void NewCharacter(int slotIndex, string raceId, string characterName)
	{
		var race = GD.Load<RaceData>($"res://data/races/{raceId}.tres");

		Active = new SaveSlotData
		{
			RaceId = raceId,
			CharacterName = characterName,
			Level = 1,
			CurrentXp = 0,
			CurrentStats = RuntimeStatBlock.FromDesignStats(race.BaseStats),
			EquippedWeaponId = null,
			EquippedSpellId = null
		};
		ActiveSlotIndex = slotIndex;
		SaveActive();
	}

	public void LoadSlot(int slotIndex)
	{
		Active = SaveManager.LoadSlot(slotIndex);
		ActiveSlotIndex = slotIndex;
	}

	public void SaveActive()
	{
		if (Active == null || ActiveSlotIndex < 0)
		{
			return;
		}

		Active.LastSavedUtc = DateTime.UtcNow.ToString("o");
		SaveManager.SaveSlot(ActiveSlotIndex, Active);
	}

	public void ApplyStatRoll(StatRollData roll) => SaveSlotMutations.ApplyStatRoll(Active, roll);

	public void EquipWeapon(string weaponId) => SaveSlotMutations.EquipWeapon(Active, weaponId);

	public void EquipSpell(string spellId) => SaveSlotMutations.EquipSpell(Active, spellId);

	public void MarkWeaponObtained(string weaponId) => SaveSlotMutations.MarkWeaponObtained(Active, weaponId);

	public void MarkSpellObtained(string spellId) => SaveSlotMutations.MarkSpellObtained(Active, spellId);

	public void RecordBattleResult(bool won, bool wasBoss)
	{
		var stats = Active.Statistics;
		stats.TotalBattlesPlayed++;
		if (won)
		{
			stats.BattlesWon++;
		}
		else
		{
			stats.BattlesLost++;
		}

		if (wasBoss && won)
		{
			stats.BossesDefeated++;
		}
	}

	public void RunSelfTest()
	{
		NewCharacter(0, "human", "Test Hero");
		var beforeStr = Active.CurrentStats.Str;
		var beforeName = Active.CharacterName;
		GD.Print($"[GameState.RunSelfTest] Created '{beforeName}' (STR={beforeStr}), saved to slot 0.");

		Active = null;
		ActiveSlotIndex = -1;

		LoadSlot(0);
		GD.Print($"[GameState.RunSelfTest] Reloaded slot 0: name='{Active.CharacterName}', STR={Active.CurrentStats.Str}, race='{Active.RaceId}', match={Active.CharacterName == beforeName && Active.CurrentStats.Str == beforeStr}");
		GD.Print($"[GameState.RunSelfTest] Save file path: {ProjectSettings.GlobalizePath("user://saves/slot_0.json")}");
	}
}
