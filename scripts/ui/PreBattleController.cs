using Godot;
using TinyTourney.Combat;

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

	// Optional: the two fighters' sprites, so the matchup shows who is actually
	// meeting whom instead of two columns of numbers (DESIGN.md §11). Leave either
	// unassigned and the screen simply loads without that portrait.
	[Export] public TextureRect PlayerPortrait;
	[Export] public TextureRect EnemyPortrait;

	// Optional: the "VS" mark between the two cards.
	[Export] public Label VsLabel;

	private static readonly Color AheadColor = new(0.6f, 0.82f, 0.53f);
	private static readonly Color BehindColor = new(0.85f, 0.42f, 0.36f);

	public override void _Ready()
	{
		var player = BattleContext.PlayerState;
		var enemy = BattleContext.EnemyState;

		ShowPortrait(PlayerPortrait, player, facesRight: true);
		ShowPortrait(EnemyPortrait, enemy, facesRight: false);

		if (VsLabel != null)
		{
			VsLabel.Text = TranslationServer.Translate("LABEL_VS");
		}

		PlayerNameLabel.Text = player.Name;
		EnemyNameLabel.Text = enemy.Name;

		// A side-by-side matchup is only useful if it says who is ahead on each stat,
		// not just what the numbers are — so every pair is coloured by comparison
		// rather than only the player's own labels being filled in.
		SetComparedStat(PlayerHpLabel, EnemyHpLabel, "STAT_HP", player.MaxHp, enemy.MaxHp);
		SetComparedStat(PlayerStrLabel, EnemyStrLabel, "STAT_STR", player.Stats.Str, enemy.Stats.Str);
		SetComparedStat(PlayerSpdLabel, EnemySpdLabel, "STAT_SPD", player.Stats.Spd, enemy.Stats.Spd);
		SetComparedStat(PlayerDurLabel, EnemyDurLabel, "STAT_DUR", player.Stats.Dur, enemy.Stats.Dur);
		SetComparedStat(PlayerDexLabel, EnemyDexLabel, "STAT_DEX", player.Stats.Dex, enemy.Stats.Dex);
		SetComparedStat(PlayerLukLabel, EnemyLukLabel, "STAT_LUK", player.Stats.Luk, enemy.Stats.Luk);
		SetComparedStat(PlayerIntLabel, EnemyIntLabel, "STAT_INT", player.Stats.Int, enemy.Stats.Int);

		PlayerWeaponLabel.Text = player.EquippedWeapon != null ? player.EquippedWeapon.DisplayName : TranslationServer.Translate("ITEM_NONE");
		PlayerSpellLabel.Text = player.EquippedSpell != null ? player.EquippedSpell.DisplayName : TranslationServer.Translate("ITEM_NONE");
		EnemyWeaponLabel.Text = enemy.EquippedWeapon != null ? enemy.EquippedWeapon.DisplayName : TranslationServer.Translate("ITEM_NONE");
		EnemySpellLabel.Text = enemy.EquippedSpell != null ? enemy.EquippedSpell.DisplayName : TranslationServer.Translate("ITEM_NONE");

		BossLabel.Visible = BattleContext.IsBoss;
		BossLabel.Text = TranslationServer.Translate("BOSS_LABEL");

		FightButton.Text = TranslationServer.Translate("BTN_FIGHT");
		FightButton.Pressed += OnFightPressed;
	}

	/// <summary>
	/// Writes both sides of one stat row and colours whichever value is ahead — green
	/// for the higher number, a warm red for the lower, the theme's normal ink colour
	/// on a tie. Comparing the two panels by eye was the whole point of this screen and
	/// neither number carried any signal about which one was actually better.
	/// </summary>
	private void SetComparedStat(Label mine, Label theirs, string statKey, int myValue, int theirValue)
	{
		string label = TranslationServer.Translate(statKey);
		mine.Text = $"{label}: {myValue}";
		theirs.Text = $"{label}: {theirValue}";

		if (myValue > theirValue)
		{
			mine.AddThemeColorOverride("font_color", AheadColor);
			theirs.AddThemeColorOverride("font_color", BehindColor);
		}
		else if (myValue < theirValue)
		{
			mine.AddThemeColorOverride("font_color", BehindColor);
			theirs.AddThemeColorOverride("font_color", AheadColor);
		}
		else
		{
			mine.RemoveThemeColorOverride("font_color");
			theirs.RemoveThemeColorOverride("font_color");
		}
	}

	private static void ShowPortrait(TextureRect portrait, CombatantState state, bool facesRight)
	{
		if (portrait == null)
		{
			return;
		}

		portrait.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
		portrait.FlipH = !facesRight;

		var race = state?.Race;
		if (race?.ReferenceImagePath is { Length: > 0 } path && ResourceLoader.Exists(path))
		{
			portrait.Texture = GD.Load<Texture2D>(path);
		}
		else
		{
			portrait.Texture = null;
			GD.PushWarning($"[PreBattleController] No portrait for race '{race?.Id ?? "null"}'.");
		}
	}

	private void OnFightPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/screens/battle_controller.tscn");
	}
}
