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

	public override void _Ready()
	{
		RefreshLabels();
		FightButton.Pressed += OnFightPressed;
	}

	private void RefreshLabels()
	{
		var save = GameState.Instance.Active;
		var stats = save.CurrentStats;

		CharacterNameLabel.Text = save.CharacterName;
		LevelLabel.Text = save.Level.ToString();
		XpLabel.Text = $"{save.CurrentXp}/{XpCurve.XpRequiredForLevel(save.Level)}";
		StrLabel.Text = $"{TranslationServer.Translate("STAT_STR")}: {stats.Str}";
		SpdLabel.Text = $"{TranslationServer.Translate("STAT_SPD")}: {stats.Spd}";
		DurLabel.Text = $"{TranslationServer.Translate("STAT_DUR")}: {stats.Dur}";
		DexLabel.Text = $"{TranslationServer.Translate("STAT_DEX")}: {stats.Dex}";
		LukLabel.Text = $"{TranslationServer.Translate("STAT_LUK")}: {stats.Luk}";
		IntLabel.Text = $"{TranslationServer.Translate("STAT_INT")}: {stats.Int}";
		EquippedWeaponLabel.Text = save.EquippedWeaponId != null ? ContentRepository.GetWeaponById(save.EquippedWeaponId).DisplayName : TranslationServer.Translate("ITEM_NONE");
		EquippedSpellLabel.Text = save.EquippedSpellId != null ? ContentRepository.GetSpellById(save.EquippedSpellId).DisplayName : TranslationServer.Translate("ITEM_NONE");
		BattlesWonLabel.Text = save.Statistics.BattlesWon.ToString();
		TotalBattlesLabel.Text = save.Statistics.TotalBattlesPlayed.ToString();
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
