namespace TinyTourney.Combat;

public enum CombatEventType
{
    RoundStart,
    WeaponDropped,
    SpellCast,
    SpellFallbackToWeapon,
    AttackHit,
    AttackMiss,
    AttackBlocked,
    AttackCountered,
    AttackPaidBack,
    Defeated,
    BattleTimeout
}
