using System.Collections.Generic;
using System.Linq;
using Godot;
using TinyTourney.Combat;
using TinyTourney.Core;
using TinyTourney.Data;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class MainController : Control
{
	[Export] public Label CharacterNameLabel;
	[Export] public Label LevelLabel;
	[Export] public Label XpLabel;
	[Export] public Label StrLabel;
	[Export] public Label SpdLabel;
	[Export] public Label DurLabel;
	[Export] public Label DexLabel;
	[Export] public Label LukLabel;
	[Export] public Label IntLabel;
	[Export] public Label EquippedWeaponLabel;
	[Export] public Label EquippedSpellLabel;
	[Export] public Label BattlesWonLabel;
	[Export] public Label TotalBattlesLabel;
	[Export] public Button FightButton;

	/// <summary>
	/// Opens the settings screen. Optional: leave it unassigned and the main screen simply
	/// has no way through to settings, rather than failing to load.
	/// </summary>
	[Export] public Button SettingsButton;

	// Everything below is optional. The screen was built as a flat column of labels, and
	// these are the pieces that turn it into something with a shape. Any of them can be
	// left unassigned while the scene is being rebuilt — the screen still loads, it just
	// shows less.

	/// <summary>The character's own sprite, so the screen shows who you are playing.</summary>
	[Export] public TextureRect CharacterPortrait;

	/// <summary>Race name, sitting under the character's name.</summary>
	[Export] public Label RaceLabel;

	/// <summary>Progress toward the next level, as a bar rather than only "3/16".</summary>
	[Export] public ProgressBar XpBar;

	[Export] public Label AttributesHeaderLabel;
	[Export] public Label EquipmentHeaderLabel;

	/// <summary>The equipped weapon's icon, meant to sit inside an IconSlot socket.</summary>
	[Export] public TextureRect WeaponIcon;

	/// <summary>The equipped spell's icon, meant to sit inside an IconSlot socket.</summary>
	[Export] public TextureRect SpellIcon;

	public override void _Ready()
	{
		RefreshLabels();
		FightButton.Pressed += OnFightPressed;

		if (SettingsButton != null)
		{
			SettingsButton.Text = TranslationServer.Translate("BTN_SETTINGS");
			SettingsButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/screens/settings.tscn");
		}

		if (AttributesHeaderLabel != null)
		{
			AttributesHeaderLabel.Text = TranslationServer.Translate("SECTION_ATTRIBUTES");
		}

		if (EquipmentHeaderLabel != null)
		{
			EquipmentHeaderLabel.Text = TranslationServer.Translate("SECTION_EQUIPMENT");
		}
	}

	private void RefreshLabels()
	{
		var save = GameState.Instance.Active;
		var stats = save.CurrentStats;

		int xpNeeded = XpCurve.XpRequiredForLevel(save.Level);

		CharacterNameLabel.Text = save.CharacterName;
		LevelLabel.Text = $"{TranslationServer.Translate("LABEL_LEVEL")}: {save.Level}";
		XpLabel.Text = $"{TranslationServer.Translate("LABEL_XP")}: {save.CurrentXp}/{xpNeeded}";

		var race = ContentRepository.GetRaceById(save.RaceId);

		if (RaceLabel != null)
		{
			RaceLabel.Text = race != null
				? TranslationServer.Translate($"RACE_{race.Id.ToUpper()}")
				: save.RaceId;
		}

		if (CharacterPortrait != null)
		{
			ShowPortrait(race);
		}

		if (XpBar != null)
		{
			XpBar.MaxValue = xpNeeded;
			XpBar.Value = save.CurrentXp;
		}
		StrLabel.Text = $"{TranslationServer.Translate("STAT_STR")}: {stats.Str}";
		SpdLabel.Text = $"{TranslationServer.Translate("STAT_SPD")}: {stats.Spd}";
		DurLabel.Text = $"{TranslationServer.Translate("STAT_DUR")}: {stats.Dur}";
		DexLabel.Text = $"{TranslationServer.Translate("STAT_DEX")}: {stats.Dex}";
		LukLabel.Text = $"{TranslationServer.Translate("STAT_LUK")}: {stats.Luk}";
		IntLabel.Text = $"{TranslationServer.Translate("STAT_INT")}: {stats.Int}";
		var weapon = save.EquippedWeaponId != null ? ContentRepository.GetWeaponById(save.EquippedWeaponId) : null;
		var spell = save.EquippedSpellId != null ? ContentRepository.GetSpellById(save.EquippedSpellId) : null;

		string weaponName = weapon != null ? weapon.DisplayName : TranslationServer.Translate("ITEM_NONE");
		string spellName = spell != null ? spell.DisplayName : TranslationServer.Translate("ITEM_NONE");

		EquippedWeaponLabel.Text = $"{TranslationServer.Translate("LABEL_WEAPON")}: {weaponName}";
		EquippedSpellLabel.Text = $"{TranslationServer.Translate("LABEL_SPELL")}: {spellName}";

		ShowIcon(WeaponIcon, weapon?.IconPath);
		ShowIcon(SpellIcon, spell?.IconPath);
		BattlesWonLabel.Text = $"{TranslationServer.Translate("LABEL_BATTLES_WON")}: {save.Statistics.BattlesWon}";
		TotalBattlesLabel.Text = $"{TranslationServer.Translate("LABEL_TOTAL_BATTLES")}: {save.Statistics.TotalBattlesPlayed}";

		// Owned by the script rather than the scene, so the label follows the chosen language.
		FightButton.Text = TranslationServer.Translate("BTN_FIGHT");
	}

	/// <summary>
	/// Puts the character's race sprite in the portrait slot. The same image the fighter
	/// uses in battle, so the character on this screen is recognisably the one who walks
	/// into the arena.
	/// </summary>
	private void ShowPortrait(RaceData race)
	{
		// Keep the pixel art crisp regardless of the project-wide filter.
		CharacterPortrait.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

		if (race?.ReferenceImagePath is { Length: > 0 } path && ResourceLoader.Exists(path))
		{
			CharacterPortrait.Texture = GD.Load<Texture2D>(path);
		}
		else
		{
			CharacterPortrait.Texture = null;
			GD.PushWarning($"[MainController] No portrait for race '{race?.Id ?? "null"}'.");
		}
	}

	/// <summary>
	/// Loads a weapon or spell's icon into its socket. Empty-handed (no weapon or spell
	/// equipped yet) is a real, expected state — the icon is just cleared, not an error.
	/// </summary>
	private static void ShowIcon(TextureRect slot, string iconPath)
	{
		if (slot == null)
		{
			return;
		}

		slot.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

		if (iconPath is { Length: > 0 } path && ResourceLoader.Exists(path))
		{
			slot.Texture = GD.Load<Texture2D>(path);
			slot.Visible = true;
		}
		else
		{
			slot.Texture = null;
			slot.Visible = false;
		}
	}

	public List<(WeaponData Data, bool Unlocked)> GetWeaponMenu()
	{
		var save = GameState.Instance.Active;
		return ContentRepository.AllWeapons
			.OrderBy(w => w.Tier)
			.Select(w => (w, save.ObtainedWeaponIds.Contains(w.Id)))
			.ToList();
	}

	public List<(SpellData Data, bool Unlocked)> GetSpellMenu()
	{
		var save = GameState.Instance.Active;
		return ContentRepository.AllSpells
			.OrderBy(s => s.Tier)
			.Select(s => (s, save.ObtainedSpellIds.Contains(s.Id)))
			.ToList();
	}

	private void OnFightPressed()
	{
		var save = GameState.Instance.Active;
		var weapon = save.EquippedWeaponId != null ? ContentRepository.GetWeaponById(save.EquippedWeaponId) : null;
		var spell = save.EquippedSpellId != null ? ContentRepository.GetSpellById(save.EquippedSpellId) : null;

		var player = new CombatantState(save.CharacterName, save.CurrentStats, weapon, spell)
		{
			Race = ContentRepository.GetRaceById(save.RaceId)
		};
		var (enemy, isBoss) = EnemyFactory.CreateEnemy(save);

		BattleContext.PlayerState = player;
		BattleContext.EnemyState = enemy;
		BattleContext.IsBoss = isBoss;

		GetTree().ChangeSceneToFile("res://scenes/screens/pre_battle_controller.tscn");
	}
}
