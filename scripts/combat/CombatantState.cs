using TinyTourney.Core;
using TinyTourney.Data;

namespace TinyTourney.Combat;

public class CombatantState
{
    private const int BaseHp = 50;
    private const int HpPerDur = 8;
    private const int ManaPerInt = 6;

    public string Name { get; }
    public RuntimeStatBlock Stats { get; }

    public int MaxHp { get; }
    public int CurrentHp { get; set; }
    public int MaxMana { get; }
    public int CurrentMana { get; set; }

    public WeaponData EquippedWeapon { get; }
    public SpellData EquippedSpell { get; }

    public bool HasWeaponDropped { get; set; }
    public float SpellCooldownRemaining { get; set; }

    public bool IsDefeated => CurrentHp <= 0;

    public CombatantState(string name, RuntimeStatBlock stats, WeaponData equippedWeapon, SpellData equippedSpell)
    {
        Name = name;
        Stats = stats;
        EquippedWeapon = equippedWeapon;
        EquippedSpell = equippedSpell;

        MaxHp = BaseHp + stats.Dur * HpPerDur;
        CurrentHp = MaxHp;
        MaxMana = stats.Int * ManaPerInt;
        CurrentMana = MaxMana;
    }
}
