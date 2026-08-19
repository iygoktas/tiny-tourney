using System;
using System.Linq;
using Godot;
using TinyTourney.Data;
using TinyTourney.Progression;

namespace TinyTourney.Core;

public partial class GameState : Node
{
	public static GameState Instance { get; private set; }

	public int ActiveSlotIndex { get; private set; } = -1;
	public SaveSlotData Active { get; private set; }

	public override void _Ready()
	{
		Instance = this;

		// RunSelfTest() is NOT called here on purpose. It writes a "Test Hero" character
		// into slot 0, so running it on every launch quietly destroyed whatever the player
		// had saved there and left them one slot short. Call it by hand when debugging.
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
			CurrentStats = RuntimeStatBlock.FromDesignStats(race.BaseStats)
		};

		GrantStartingGear(Active);

		ActiveSlotIndex = slotIndex;
		SaveActive();
	}

	/// <summary>
	/// Arms a brand new character with the entry-level weapon and spell.
	/// Opponents are always generated holding both, so starting empty-handed left the
	/// player unable to win a single fight — and unable to reach the level-up that would
	/// have handed them their first weapon.
	///
	/// The items are picked from the content tables rather than named here, so rebalancing
	/// which item is the starter is a data change.
	/// </summary>
	private static void GrantStartingGear(SaveSlotData save)
	{
		var starterWeapon = ContentRepository.AllWeapons
			.Where(w => w.MinLevel <= 1)
			.OrderBy(w => w.Tier)
			.ThenBy(w => w.MinLevel)
			.FirstOrDefault();

		var starterSpell = ContentRepository.AllSpells
			.Where(s => s.MinLevel <= 1)
			.OrderBy(s => s.Tier)
			.ThenBy(s => s.MinLevel)
			.FirstOrDefault();

		if (starterWeapon != null)
		{
			SaveSlotMutations.EquipWeapon(save, starterWeapon.Id);
			// Marked as obtained so the level-up wheel never offers it a second time.
			SaveSlotMutations.MarkWeaponObtained(save, starterWeapon.Id);
		}

		if (starterSpell != null)
		{
			SaveSlotMutations.EquipSpell(save, starterSpell.Id);
			SaveSlotMutations.MarkSpellObtained(save, starterSpell.Id);
		}
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
