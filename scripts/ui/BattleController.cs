using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TinyTourney.Combat;
using TinyTourney.Core;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class BattleController : Control
{
	[Export] public ProgressBar PlayerHpBar;
	[Export] public ProgressBar EnemyHpBar;
	[Export] public RichTextLabel LogLabel;
	[Export] public Button Speed1xButton;
	[Export] public Button Speed2xButton;
	[Export] public Button Speed4xButton;
	[Export] public Button SkipButton;

	private const float BaseDelaySeconds = 0.6f;

	private CombatantState _player;
	private CombatantState _enemy;
	private bool _isBoss;
	private List<CombatEvent> _events;
	private float _speedMultiplier = 1f;
	private bool _skipRequested;

	public override void _Ready()
	{
		_player = BattleContext.PlayerState;
		_enemy = BattleContext.EnemyState;
		_isBoss = BattleContext.IsBoss;

		PlayerHpBar.MaxValue = _player.MaxHp;
		EnemyHpBar.MaxValue = _enemy.MaxHp;
		PlayerHpBar.Value = _player.MaxHp;
		EnemyHpBar.Value = _enemy.MaxHp;

		Speed1xButton.Pressed += () => _speedMultiplier = 1f;
		Speed2xButton.Pressed += () => _speedMultiplier = 2f;
		Speed4xButton.Pressed += () => _speedMultiplier = 4f;
		SkipButton.Pressed += () => _skipRequested = true;

		_events = CombatEngine.RunBattle(_player, _enemy);
		_ = PlayEvents();
	}

	private async Task PlayEvents()
	{
		int playerHp = _player.MaxHp;
		int enemyHp = _enemy.MaxHp;

		foreach (var evt in _events)
		{
			ApplyEventToLog(evt);
			(playerHp, enemyHp) = ApplyEventToHp(evt, playerHp, enemyHp);
			PlayerHpBar.Value = playerHp;
			EnemyHpBar.Value = enemyHp;

			if (!_skipRequested)
			{
				float delay = BaseDelaySeconds / _speedMultiplier;
				await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
			}
		}

		OnBattleComplete();
	}

	private (int PlayerHp, int EnemyHp) ApplyEventToHp(CombatEvent evt, int playerHp, int enemyHp)
	{
		int amount = (int)evt.Amount;

		switch (evt.EventType)
		{
			case CombatEventType.AttackHit:
			case CombatEventType.AttackCountered:
				if (evt.TargetName == _player.Name) playerHp -= amount;
				else if (evt.TargetName == _enemy.Name) enemyHp -= amount;
				break;
			case CombatEventType.AttackPaidBack:
				if (evt.ActorName == _player.Name) playerHp -= amount;
				else if (evt.ActorName == _enemy.Name) enemyHp -= amount;
				if (evt.TargetName == _player.Name) playerHp -= amount;
				else if (evt.TargetName == _enemy.Name) enemyHp -= amount;
				break;
		}

		return (Math.Max(0, playerHp), Math.Max(0, enemyHp));
	}

	private void ApplyEventToLog(CombatEvent evt)
	{
		string line = evt.EventType switch
		{
			CombatEventType.AttackMiss => $"{evt.ActorName}: {TranslationServer.Translate("COMBAT_MISS")}",
			CombatEventType.AttackBlocked => $"{evt.TargetName}: {TranslationServer.Translate("COMBAT_BLOCKED")}",
			CombatEventType.AttackCountered => $"{evt.ActorName}: {TranslationServer.Translate("COMBAT_COUNTERED")}",
			CombatEventType.AttackPaidBack => $"{evt.ActorName}: {TranslationServer.Translate("COMBAT_PAID_BACK")}",
			CombatEventType.AttackHit => evt.IsCritical
				? $"{evt.ActorName} -> {evt.TargetName}: {evt.Amount:F0} {TranslationServer.Translate("COMBAT_CRITICAL")}"
				: $"{evt.ActorName} -> {evt.TargetName}: {evt.Amount:F0}",
			CombatEventType.SpellCast => $"{evt.ActorName} casts a spell",
			CombatEventType.WeaponDropped => $"{evt.ActorName}'s weapon dropped!",
			CombatEventType.Defeated => $"{evt.ActorName} defeated!",
			CombatEventType.BattleTimeout => "Battle timed out",
			_ => null
		};

		if (line != null)
		{
			LogLabel.AppendText(line + "\n");
		}
	}

	private void OnBattleComplete()
	{
		bool won = !_player.IsDefeated;
		GameState.Instance.RecordBattleResult(won, _isBoss);
		var levelUps = ProgressionManager.AwardXp(GameState.Instance.Active, won);
		GameState.Instance.SaveActive();

		if (levelUps.Count > 0)
		{
			WheelContext.PendingResults = new Queue<LevelUpResult>(levelUps);
			GetTree().ChangeSceneToFile("res://scenes/screens/Wheel.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://scenes/screens/Main.tscn");
		}
	}
}
