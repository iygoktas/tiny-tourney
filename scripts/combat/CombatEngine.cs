using System;
using System.Collections.Generic;
using Godot;
using TinyTourney.Core;
using TinyTourney.Data;

namespace TinyTourney.Combat;

public static class CombatEngine
{
	private const int MaxRounds = 500;
	private static readonly float[] MultiHitFalloff = { 1.0f, 0.75f, 0.5f, 0.25f, 0.10f };
	private static readonly Random Rng = new();

	public static List<CombatEvent> RunBattle(CombatantState side1, CombatantState side2)
	{
		var events = new List<CombatEvent>();
		int round = 1;
		
		while (round <= MaxRounds)
		{
			events.Add(new CombatEvent { EventType = CombatEventType.RoundStart, RoundNumber = round });

			CombatantState attacker = side1.Stats.Spd >= side2.Stats.Spd ? side1 : side2;
			CombatantState defender = attacker == side1 ? side2 : side1;

			ResolveTurn(attacker, defender, events, round);
			if (TryLogDefeat(side1, events, round) | TryLogDefeat(side2, events, round))
			{
				return events;
			}

			ResolveTurn(defender, attacker, events, round);
			if (TryLogDefeat(side1, events, round) | TryLogDefeat(side2, events, round))
			{
				return events;
			}

			round++;
		}

		events.Add(new CombatEvent { EventType = CombatEventType.BattleTimeout, RoundNumber = round });
		return events;
	}

	private static bool TryLogDefeat(CombatantState combatant, List<CombatEvent> events, int round)
	{
		if (!combatant.IsDefeated)
		{
			return false;
		}

		events.Add(new CombatEvent { EventType = CombatEventType.Defeated, ActorName = combatant.Name, RoundNumber = round });
		return true;
	}

	private static void ResolveTurn(CombatantState attacker, CombatantState defender, List<CombatEvent> events, int round)
	{
		if (!attacker.HasWeaponDropped && attacker.EquippedWeapon != null)
		{
			double dropChance = attacker.Stats.Luk * 0.01 + round * 0.05;
			if (Rng.NextDouble() < dropChance)
			{
				attacker.HasWeaponDropped = true;
				events.Add(new CombatEvent { EventType = CombatEventType.WeaponDropped, ActorName = attacker.Name, RoundNumber = round });
			}
		}

		attacker.SpellCooldownRemaining = Math.Max(0, attacker.SpellCooldownRemaining - 1);

		bool spellReady = attacker.EquippedSpell != null && attacker.SpellCooldownRemaining <= 0;
		if (spellReady && attacker.CurrentMana >= attacker.EquippedSpell.ManaCost)
		{
			ResolveSpellCast(attacker, defender, events, round);
			return;
		}

		if (spellReady)
		{
			events.Add(new CombatEvent { EventType = CombatEventType.SpellFallbackToWeapon, ActorName = attacker.Name, RoundNumber = round });
		}

		ResolveWeaponAttack(attacker, defender, events, round);
	}

	private static void ResolveWeaponAttack(CombatantState attacker, CombatantState defender, List<CombatEvent> events, int round)
	{
		bool hasWeapon = attacker.HasWeaponDropped && attacker.EquippedWeapon != null;
		AttackType attackType = hasWeapon
			? (Rng.NextDouble() < 0.5 ? AttackType.Normal : AttackType.Thrust)
			: AttackType.Fist;

		int speedDelta = Math.Max(0, attacker.Stats.Spd - defender.Stats.Spd);
		int totalHits = 1 + Math.Min(4, speedDelta / 10);

		for (int i = 0; i < totalHits; i++)
		{
			if (defender.IsDefeated || attacker.IsDefeated)
			{
				break;
			}

			bool isCritical = hasWeapon && Rng.NextDouble() < attacker.EquippedWeapon.CritChance + attacker.Stats.Luk * 0.005;
			float rawDamage = ComputeWeaponHitDamage(attacker, attackType) * MultiHitFalloff[i];
			if (isCritical)
			{
				rawDamage *= attacker.EquippedWeapon.CritMultiplier;
			}

			ResolveDefenseSequence(attacker, defender, rawDamage, DamageType.Physical, attackType, isCritical, events, round);
		}
	}

	private static float ComputeWeaponHitDamage(CombatantState attacker, AttackType attackType)
	{
		if (attackType == AttackType.Fist)
		{
			return 3f + attacker.Stats.Str * 0.2f;
		}

		var weapon = attacker.EquippedWeapon;
		float baseDamage = attackType == AttackType.Thrust ? weapon.ThrustDamage : weapon.NormalDamage;

		return baseDamage + attacker.Stats.Str * weapon.StrScaling;
	}

	private static void ResolveSpellCast(CombatantState attacker, CombatantState defender, List<CombatEvent> events, int round)
	{
		var spell = attacker.EquippedSpell;
		attacker.CurrentMana -= spell.ManaCost;
		attacker.SpellCooldownRemaining = spell.Cooldown;

		events.Add(new CombatEvent
		{
			EventType = CombatEventType.SpellCast,
			ActorName = attacker.Name,
			TargetName = defender.Name,
			AttackType = AttackType.Spell,
			DamageType = spell.DamageType,
			RoundNumber = round
		});

		float damage = spell.BaseDamage + attacker.Stats.Int * 1.5f;
		ResolveDefenseSequence(attacker, defender, damage, spell.DamageType, AttackType.Spell, false, events, round);
	}

	private static void ResolveDefenseSequence(CombatantState attacker, CombatantState defender, float rawDamage, DamageType damageType, AttackType attackType, bool isCritical, List<CombatEvent> events, int round)
	{
		double missChance = Math.Max(0, 0.20 - attacker.Stats.Dex * 0.005);
		if (Rng.NextDouble() < missChance)
		{
			events.Add(new CombatEvent { EventType = CombatEventType.AttackMiss, ActorName = attacker.Name, TargetName = defender.Name, AttackType = attackType, DamageType = damageType, RoundNumber = round });
			return;
		}

		double counterChance = Math.Min(0.15, defender.Stats.Dex * 0.005);
		if (Rng.NextDouble() < counterChance)
		{
			attacker.CurrentHp -= (int)rawDamage;
			events.Add(new CombatEvent { EventType = CombatEventType.AttackCountered, ActorName = defender.Name, TargetName = attacker.Name, AttackType = attackType, DamageType = damageType, Amount = rawDamage, IsCritical = isCritical, RoundNumber = round });
			return;
		}

		double paybackChance = Math.Min(0.20, defender.Stats.Dex * 0.0075);
		if (Rng.NextDouble() < paybackChance)
		{
			defender.CurrentHp -= (int)rawDamage;
			attacker.CurrentHp -= (int)rawDamage;
			events.Add(new CombatEvent { EventType = CombatEventType.AttackPaidBack, ActorName = defender.Name, TargetName = attacker.Name, AttackType = attackType, DamageType = damageType, Amount = rawDamage, IsCritical = isCritical, RoundNumber = round });
			return;
		}

		double blockChance = Math.Min(0.30, defender.Stats.Dex * 0.01);
		if (Rng.NextDouble() < blockChance)
		{
			events.Add(new CombatEvent { EventType = CombatEventType.AttackBlocked, ActorName = defender.Name, TargetName = attacker.Name, AttackType = attackType, DamageType = damageType, RoundNumber = round });
			return;
		}

		defender.CurrentHp -= (int)rawDamage;
		events.Add(new CombatEvent { EventType = CombatEventType.AttackHit, ActorName = attacker.Name, TargetName = defender.Name, AttackType = attackType, DamageType = damageType, Amount = rawDamage, IsCritical = isCritical, RoundNumber = round });
	}

	public static void RunSelfTest()
	{
		var humanRace = GD.Load<RaceData>("res://data/races/human.tres");
		var orcRace = GD.Load<RaceData>("res://data/races/orc.tres");
		var w01 = GD.Load<WeaponData>("res://data/weapons/w01_wooden_club.tres");
		var w02 = GD.Load<WeaponData>("res://data/weapons/w02_bronze_shortsword.tres");
		var sp01 = GD.Load<SpellData>("res://data/spells/sp01_magic_missile.tres");
		var sp02 = GD.Load<SpellData>("res://data/spells/sp02_static_shock.tres");

		var player = new CombatantState("Human Hero", RuntimeStatBlock.FromDesignStats(humanRace.BaseStats), w01, sp01);
		var enemy = new CombatantState("Orc Enemy", RuntimeStatBlock.FromDesignStats(orcRace.BaseStats), w02, sp02);

		var events = RunBattle(player, enemy);

		foreach (var e in events)
		{
			GD.Print($"[Round {e.RoundNumber}] {e.EventType} actor={e.ActorName} target={e.TargetName} attackType={e.AttackType} damageType={e.DamageType} amount={e.Amount:F1}");
		}

		string winner = player.IsDefeated ? enemy.Name : enemy.IsDefeated ? player.Name : "no one (timeout)";
		GD.Print($"[CombatEngine.RunSelfTest] Winner: {winner}. Player HP={player.CurrentHp}/{player.MaxHp}, Enemy HP={enemy.CurrentHp}/{enemy.MaxHp}");
	}
}
