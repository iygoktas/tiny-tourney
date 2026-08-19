using System;
using System.Collections.Generic;
using System.Linq;
using TinyTourney.Core;

namespace TinyTourney.Progression;

public static class WheelSpinner
{
    private static readonly Random Rng = new();

    public static WheelResult Spin(SaveSlotData save)
    {
        int level = save.Level;

        var eligibleWeapons = ContentRepository.AllWeapons
            .Where(w => w.MinLevel <= level && !save.ObtainedWeaponIds.Contains(w.Id))
            .ToList();
        var eligibleSpells = ContentRepository.AllSpells
            .Where(s => s.MinLevel <= level && !save.ObtainedSpellIds.Contains(s.Id))
            .ToList();
        var eligibleStatRolls = ContentRepository.AllStatRolls
            .Where(s => s.MinLevel <= level)
            .ToList();

        bool allWeaponsObtained = ContentRepository.AllWeapons.Count > 0
            && ContentRepository.AllWeapons.All(w => save.ObtainedWeaponIds.Contains(w.Id));
        bool allSpellsObtained = ContentRepository.AllSpells.Count > 0
            && ContentRepository.AllSpells.All(s => save.ObtainedSpellIds.Contains(s.Id));

        if (allWeaponsObtained && allSpellsObtained)
        {
            return new WheelResult
            {
                Category = WheelCategory.Stat,
                StatRoll = PickWeighted(eligibleStatRolls, r => r.Tier),
                IsEndgameMode = true
            };
        }

        var offeredCategories = new List<WheelCategory> { WheelCategory.Stat };
        if (eligibleWeapons.Count > 0)
        {
            offeredCategories.Add(WheelCategory.Weapon);
        }
        if (eligibleSpells.Count > 0)
        {
            offeredCategories.Add(WheelCategory.Spell);
        }

        var category = offeredCategories[Rng.Next(offeredCategories.Count)];

        return category switch
        {
            WheelCategory.Weapon => new WheelResult { Category = WheelCategory.Weapon, Weapon = PickWeighted(eligibleWeapons, w => w.Tier) },
            WheelCategory.Spell => new WheelResult { Category = WheelCategory.Spell, Spell = PickWeighted(eligibleSpells, s => s.Tier) },
            _ => new WheelResult { Category = WheelCategory.Stat, StatRoll = PickWeighted(eligibleStatRolls, r => r.Tier) }
        };
    }

    private static T PickWeighted<T>(List<T> items, Func<T, int> tierSelector)
    {
        if (items.Count == 0)
        {
            return default;
        }

        var weights = items.Select(item => 1.0 / Math.Max(1, tierSelector(item))).ToList();
        double totalWeight = weights.Sum();
        double roll = Rng.NextDouble() * totalWeight;

        double cumulative = 0;
        for (int i = 0; i < items.Count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
            {
                return items[i];
            }
        }

        return items[^1];
    }
}
