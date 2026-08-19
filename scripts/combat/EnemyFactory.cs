using System;
using System.Linq;
using Godot;
using TinyTourney.Core;
using TinyTourney.Data;
using TinyTourney.Progression;

namespace TinyTourney.Combat;

public static class EnemyFactory
{
    private static readonly Random Rng = new();

    public static (CombatantState Enemy, bool IsBoss) CreateEnemy(SaveSlotData playerSave)
    {
        int levelDelta = Rng.Next(-1, 2);
        int enemyLevel = Math.Max(1, playerSave.Level + levelDelta);
        bool isBoss = (playerSave.Statistics.TotalBattlesPlayed + 1) % 10 == 0;

        var races = ContentRepository.AllRaces;
        var race = races[Rng.Next(races.Count)];

        var stats = new RuntimeStatBlock
        {
            Str = race.BaseStats.Str + (enemyLevel - 1),
            Spd = race.BaseStats.Spd + (enemyLevel - 1),
            Dur = race.BaseStats.Dur + (enemyLevel - 1),
            Dex = race.BaseStats.Dex + (enemyLevel - 1),
            Luk = race.BaseStats.Luk + (enemyLevel - 1),
            Int = race.BaseStats.Int + (enemyLevel - 1)
        };

        if (isBoss)
        {
            stats.Str = (int)(stats.Str * 1.5f);
            stats.Spd = (int)(stats.Spd * 1.5f);
            stats.Dur = (int)(stats.Dur * 1.5f);
            stats.Dex = (int)(stats.Dex * 1.5f);
            stats.Luk = (int)(stats.Luk * 1.5f);
            stats.Int = (int)(stats.Int * 1.5f);
        }

        var eligibleWeapons = ContentRepository.AllWeapons.Where(w => w.MinLevel <= enemyLevel).ToList();
        var eligibleSpells = ContentRepository.AllSpells.Where(s => s.MinLevel <= enemyLevel).ToList();

        WeaponData weapon = eligibleWeapons.Count > 0 ? eligibleWeapons[Rng.Next(eligibleWeapons.Count)] : null;
        SpellData spell = eligibleSpells.Count > 0 ? eligibleSpells[Rng.Next(eligibleSpells.Count)] : null;

        string name = isBoss ? $"{race.DisplayName} Boss" : race.DisplayName;

        return (new CombatantState(name, stats, weapon, spell) { Race = race }, isBoss);
    }

    public static void RunSelfTest()
    {
        for (int battleCount = 0; battleCount < 12; battleCount++)
        {
            var save = new SaveSlotData
            {
                Level = 5,
                Statistics = new BattleStatistics { TotalBattlesPlayed = battleCount }
            };

            var (enemy, isBoss) = CreateEnemy(save);
            string weaponName = enemy.EquippedWeapon != null ? enemy.EquippedWeapon.DisplayName : "none";
            string spellName = enemy.EquippedSpell != null ? enemy.EquippedSpell.DisplayName : "none";
            GD.Print($"[EnemyFactory.RunSelfTest] battle#{battleCount + 1} -> {enemy.Name} (boss={isBoss}), STR={enemy.Stats.Str}, SPD={enemy.Stats.Spd}, MaxHp={enemy.MaxHp}, Weapon={weaponName}, Spell={spellName}");
        }
    }
}
