using TinyTourney.Data;

namespace TinyTourney.Core;

public static class SaveSlotMutations
{
    public static void ApplyStatRoll(SaveSlotData save, StatRollData roll)
    {
        var stats = save.CurrentStats;
        int amount = (int)roll.Amount;

        switch (roll.Stat)
        {
            case StatType.Str:
                stats.Str += amount;
                break;
            case StatType.Spd:
                stats.Spd += amount;
                break;
            case StatType.Dur:
                stats.Dur += amount;
                break;
            case StatType.Dex:
                stats.Dex += amount;
                break;
            case StatType.Luk:
                stats.Luk += amount;
                break;
            case StatType.Int:
                stats.Int += amount;
                break;
        }
    }

    public static void EquipWeapon(SaveSlotData save, string weaponId)
    {
        save.EquippedWeaponId = weaponId;
    }

    public static void EquipSpell(SaveSlotData save, string spellId)
    {
        save.EquippedSpellId = spellId;
    }

    public static void MarkWeaponObtained(SaveSlotData save, string weaponId)
    {
        if (!save.ObtainedWeaponIds.Contains(weaponId))
        {
            save.ObtainedWeaponIds.Add(weaponId);
        }
    }

    public static void MarkSpellObtained(SaveSlotData save, string spellId)
    {
        if (!save.ObtainedSpellIds.Contains(spellId))
        {
            save.ObtainedSpellIds.Add(spellId);
        }
    }
}
