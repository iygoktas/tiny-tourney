using Godot;

namespace TinyTourney.UI;

public partial class PreBattleController : Control
{
	[Export] public Label PlayerNameLabel;
	[Export] public Label PlayerHpLabel;
	[Export] public Label PlayerStrLabel;
	[Export] public Label PlayerSpdLabel;
	[Export] public Label PlayerDurLabel;
	[Export] public Label PlayerDexLabel;
	[Export] public Label PlayerLukLabel;
	[Export] public Label PlayerIntLabel;
	[Export] public Label PlayerWeaponLabel;
	[Export] public Label PlayerSpellLabel;

	[Export] public Label EnemyNameLabel;
	[Export] public Label EnemyHpLabel;
	[Export] public Label EnemyStrLabel;
	[Export] public Label EnemySpdLabel;
	[Export] public Label EnemyDurLabel;
	[Export] public Label EnemyDexLabel;
	[Export] public Label EnemyLukLabel;
	[Export] public Label EnemyIntLabel;
	[Export] public Label EnemyWeaponLabel;
	[Export] public Label EnemySpellLabel;

	[Export] public Label BossLabel;
	[Export] public Button FightButton;

	public override void _Ready()
	{
		var player = BattleContext.PlayerState;
		var enemy = BattleContext.EnemyState;

		PlayerNameLabel.Text = player.Name;
		PlayerHpLabel.Text = $"{TranslationServer.Translate("STAT_HP")}: {player.MaxHp}";
		PlayerStrLabel.Text = $"{TranslationServer.Translate("STAT_STR")}: {player.Stats.Str}";
		PlayerSpdLabel.Text = $"{TranslationServer.Translate("STAT_SPD")}: {player.Stats.Spd}";
		PlayerDurLabel.Text = $"{TranslationServer.Translate("STAT_DUR")}: {player.Stats.Dur}";
		PlayerDexLabel.Text = $"{TranslationServer.Translate("STAT_DEX")}: {player.Stats.Dex}";
		PlayerLukLabel.Text = $"{TranslationServer.Translate("STAT_LUK")}: {player.Stats.Luk}";
		PlayerIntLabel.Text = $"{TranslationServer.Translate("STAT_INT")}: {player.Stats.Int}";
		PlayerWeaponLabel.Text = player.EquippedWeapon != null ? player.EquippedWeapon.DisplayName : TranslationServer.Translate("ITEM_NONE");
		PlayerSpellLabel.Text = player.EquippedSpell != null ? player.EquippedSpell.DisplayName : TranslationServer.Translate("ITEM_NONE");

		EnemyNameLabel.Text = enemy.Name;
		EnemyHpLabel.Text = $"{TranslationServer.Translate("STAT_HP")}: {enemy.MaxHp}";
		EnemyStrLabel.Text = $"{TranslationServer.Translate("STAT_STR")}: {enemy.Stats.Str}";
		EnemySpdLabel.Text = $"{TranslationServer.Translate("STAT_SPD")}: {enemy.Stats.Spd}";
		EnemyDurLabel.Text = $"{TranslationServer.Translate("STAT_DUR")}: {enemy.Stats.Dur}";
		EnemyDexLabel.Text = $"{TranslationServer.Translate("STAT_DEX")}: {enemy.Stats.Dex}";
		EnemyLukLabel.Text = $"{TranslationServer.Translate("STAT_LUK")}: {enemy.Stats.Luk}";
		EnemyIntLabel.Text = $"{TranslationServer.Translate("STAT_INT")}: {enemy.Stats.Int}";
		EnemyWeaponLabel.Text = enemy.EquippedWeapon != null ? enemy.EquippedWeapon.DisplayName : TranslationServer.Translate("ITEM_NONE");
		EnemySpellLabel.Text = enemy.EquippedSpell != null ? enemy.EquippedSpell.DisplayName : TranslationServer.Translate("ITEM_NONE");

		BossLabel.Visible = BattleContext.IsBoss;

		FightButton.Pressed += OnFightPressed;
	}

	private void OnFightPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/screens/Battle.tscn");
	}
}
