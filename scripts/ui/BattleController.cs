using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TinyTourney.Combat;
using TinyTourney.Core;
using TinyTourney.Progression;

namespace TinyTourney.UI;

/// <summary>
/// Plays a finished battle back to the screen. The fight is already fully decided by
/// <see cref="CombatEngine"/> before anything is drawn, so the speed buttons only change how
/// fast the replay runs — never the outcome.
/// </summary>
public partial class BattleController : Control
{
	[Export] public ProgressBar PlayerHpBar;
	[Export] public ProgressBar EnemyHpBar;
	[Export] public RichTextLabel LogLabel;
	[Export] public Button Speed1xButton;
	[Export] public Button Speed2xButton;
	[Export] public Button Speed4xButton;
	[Export] public Button SkipButton;

	/// <summary>The player's sprite, standing on the left. Wire this in the Inspector.</summary>
	[Export] public FighterView PlayerFighter;

	/// <summary>The opponent's sprite, standing on the right. Wire this in the Inspector.</summary>
	[Export] public FighterView EnemyFighter;

	private const float BaseDelaySeconds = 0.6f;
	private const float HpDrainSeconds = 0.25f;

	private static readonly Color DamageColor = new(1f, 0.94f, 0.85f);
	private static readonly Color CriticalColor = new(1f, 0.78f, 0.25f);
	private static readonly Color MissColor = new(0.72f, 0.75f, 0.78f);
	private static readonly Color BlockColor = new(0.55f, 0.78f, 1f);
	private static readonly Color ReflectColor = new(0.72f, 0.95f, 0.6f);

	private CombatantState _player;
	private CombatantState _enemy;
	private bool _isBoss;
	private List<CombatEvent> _events;
	private float _speedMultiplier = 1f;
	private bool _skipRequested;

	private FloatingCombatText _floatingText;
	private int _playerHp;
	private int _enemyHp;

	public override void _Ready()
	{
		_player = BattleContext.PlayerState;
		_enemy = BattleContext.EnemyState;
		_isBoss = BattleContext.IsBoss;

		_playerHp = _player.MaxHp;
		_enemyHp = _enemy.MaxHp;

		PlayerHpBar.MaxValue = _player.MaxHp;
		EnemyHpBar.MaxValue = _enemy.MaxHp;
		PlayerHpBar.Value = _player.MaxHp;
		EnemyHpBar.Value = _enemy.MaxHp;

		PlayerFighter?.Setup(_player.Race, facesRight: true);
		EnemyFighter?.Setup(_enemy.Race, facesRight: false);

		_floatingText = new FloatingCombatText(this);

		Speed1xButton.Pressed += () => _speedMultiplier = 1f;
		Speed2xButton.Pressed += () => _speedMultiplier = 2f;
		Speed4xButton.Pressed += () => _speedMultiplier = 4f;
		SkipButton.Pressed += () => _skipRequested = true;

		_events = CombatEngine.RunBattle(_player, _enemy);
		_ = PlayEvents();
	}

	private async Task PlayEvents()
	{
		foreach (var evt in _events)
		{
			ApplyEventToLog(evt);

			if (_skipRequested)
			{
				ApplyEventToHp(evt);
				continue;
			}

			await PlayEventVisuals(evt);

			float delay = BaseDelaySeconds / _speedMultiplier;
			await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		}

		// The skip path fast-forwarded past every animation, so settle the bars on the truth.
		PlayerHpBar.Value = _playerHp;
		EnemyHpBar.Value = _enemyHp;

		OnBattleComplete();
	}

	/// <summary>
	/// Runs one event as motion: the attacker steps in, the hit lands with its text and the
	/// health drain, then the attacker steps back.
	/// </summary>
	private async Task PlayEventVisuals(CombatEvent evt)
	{
		// Spells are hurled from where the caster stands; only weapon strikes close the distance.
		FighterView swinger = SwingerOf(evt);

		if (swinger != null)
		{
			await swinger.LungeOutAsync(_speedMultiplier);
		}

		ApplyEventToHp(evt);
		ShowEventText(evt);
		AnimateHpBars();
		PlayFlinches(evt);

		if (evt.EventType == CombatEventType.Defeated)
		{
			FighterView loser = ViewForName(evt.ActorName);
			if (loser != null)
			{
				await loser.PlayDefeatAsync(_speedMultiplier);
			}
		}

		if (swinger != null)
		{
			await swinger.LungeBackAsync(_speedMultiplier);
		}
	}

	/// <summary>
	/// Which fighter physically swung, so the lunge moves the right one. Careful: the engine
	/// records a block, counter or payback from the defender's point of view, putting the
	/// defender in ActorName — for those three the swinger is the event's <em>target</em>.
	/// </summary>
	private FighterView SwingerOf(CombatEvent evt) => evt.EventType switch
	{
		CombatEventType.AttackHit or CombatEventType.AttackMiss => ViewForName(evt.ActorName),
		CombatEventType.AttackBlocked
			or CombatEventType.AttackCountered
			or CombatEventType.AttackPaidBack => ViewForName(evt.TargetName),
		_ => null
	};

	/// <summary>Shakes whoever actually lost health on this event.</summary>
	private void PlayFlinches(CombatEvent evt)
	{
		switch (evt.EventType)
		{
			case CombatEventType.AttackHit:
				ViewForName(evt.TargetName)?.PlayFlinch(_speedMultiplier);
				break;

			// A counter spares the defender entirely and sends the damage back at the swinger.
			case CombatEventType.AttackCountered:
				ViewForName(evt.TargetName)?.PlayFlinch(_speedMultiplier);
				break;

			// A payback hurts both: the defender still takes it, and reflects it too.
			case CombatEventType.AttackPaidBack:
				ViewForName(evt.ActorName)?.PlayFlinch(_speedMultiplier);
				ViewForName(evt.TargetName)?.PlayFlinch(_speedMultiplier);
				break;
		}
	}

	private void ShowEventText(CombatEvent evt)
	{
		int amount = (int)evt.Amount;

		switch (evt.EventType)
		{
			// ActorName swung and TargetName dodged, so the word belongs over the dodger.
			case CombatEventType.AttackMiss:
				ShowTextOver(ViewForName(evt.TargetName), Tr("COMBAT_MISS"), MissColor, 26);
				break;

			// ActorName is the defender who blocked.
			case CombatEventType.AttackBlocked:
				ShowTextOver(ViewForName(evt.ActorName), Tr("COMBAT_BLOCKED"), BlockColor, 26);
				break;

			// ActorName countered; TargetName is the swinger who eats the reflected damage.
			case CombatEventType.AttackCountered:
				ShowTextOver(ViewForName(evt.ActorName), Tr("COMBAT_COUNTERED"), ReflectColor, 28);
				ShowTextOver(ViewForName(evt.TargetName), amount.ToString(), DamageColor, 30);
				break;

			// Both sides take the hit here.
			case CombatEventType.AttackPaidBack:
				ShowTextOver(ViewForName(evt.ActorName), Tr("COMBAT_PAID_BACK"), ReflectColor, 28);
				ShowTextOver(ViewForName(evt.ActorName), amount.ToString(), DamageColor, 30);
				ShowTextOver(ViewForName(evt.TargetName), amount.ToString(), DamageColor, 30);
				break;

			case CombatEventType.AttackHit:
				bool crit = evt.IsCritical;
				FighterView victim = ViewForName(evt.TargetName);
				ShowTextOver(victim, amount.ToString(), crit ? CriticalColor : DamageColor, crit ? 44 : 32);
				if (crit)
				{
					ShowTextOver(victim, Tr("COMBAT_CRITICAL"), CriticalColor, 24);
				}
				break;
		}
	}

	private void ShowTextOver(FighterView view, string text, Color color, int fontSize)
	{
		if (view == null || string.IsNullOrEmpty(text))
		{
			return;
		}

		Vector2 anchor = view.HomeCenter - new Vector2(0f, view.Size.Y * 0.45f);
		_floatingText.Show(text, color, anchor, fontSize, _speedMultiplier);
	}

	/// <summary>Slides the bars to their new values instead of snapping, so damage is readable.</summary>
	private void AnimateHpBars()
	{
		float duration = Mathf.Max(0.02f, HpDrainSeconds / _speedMultiplier);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(PlayerHpBar, "value", _playerHp, duration);
		tween.TweenProperty(EnemyHpBar, "value", _enemyHp, duration);
	}

	private FighterView ViewForName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}

		if (name == _player.Name) return PlayerFighter;
		if (name == _enemy.Name) return EnemyFighter;
		return null;
	}

	private void ApplyEventToHp(CombatEvent evt)
	{
		int amount = (int)evt.Amount;

		switch (evt.EventType)
		{
			case CombatEventType.AttackHit:
			case CombatEventType.AttackCountered:
				if (evt.TargetName == _player.Name) _playerHp -= amount;
				else if (evt.TargetName == _enemy.Name) _enemyHp -= amount;
				break;
			case CombatEventType.AttackPaidBack:
				if (evt.ActorName == _player.Name) _playerHp -= amount;
				else if (evt.ActorName == _enemy.Name) _enemyHp -= amount;
				if (evt.TargetName == _player.Name) _playerHp -= amount;
				else if (evt.TargetName == _enemy.Name) _enemyHp -= amount;
				break;
		}

		_playerHp = Math.Max(0, _playerHp);
		_enemyHp = Math.Max(0, _enemyHp);
	}

	private void ApplyEventToLog(CombatEvent evt)
	{
		string line = evt.EventType switch
		{
			CombatEventType.AttackMiss => $"{evt.ActorName}: {Tr("COMBAT_MISS")}",
			CombatEventType.AttackBlocked => $"{evt.TargetName}: {Tr("COMBAT_BLOCKED")}",
			CombatEventType.AttackCountered => $"{evt.ActorName}: {Tr("COMBAT_COUNTERED")}",
			CombatEventType.AttackPaidBack => $"{evt.ActorName}: {Tr("COMBAT_PAID_BACK")}",
			CombatEventType.AttackHit => evt.IsCritical
				? $"{evt.ActorName} -> {evt.TargetName}: {evt.Amount:F0} {Tr("COMBAT_CRITICAL")}"
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

	private static string Tr(string key) => TranslationServer.Translate(key);

	private void OnBattleComplete()
	{
		bool won = !_player.IsDefeated;
		GameState.Instance.RecordBattleResult(won, _isBoss);
		var levelUps = ProgressionManager.AwardXp(GameState.Instance.Active, won);
		GameState.Instance.SaveActive();

		if (levelUps.Count > 0)
		{
			WheelContext.PendingResults = new Queue<LevelUpResult>(levelUps);
			GetTree().ChangeSceneToFile("res://scenes/screens/wheel_controller.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
		}
	}
}
