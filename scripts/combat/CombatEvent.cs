using TinyTourney.Data;

namespace TinyTourney.Combat;

public class CombatEvent
{
    public CombatEventType EventType { get; set; }
    public string ActorName { get; set; }
    public string TargetName { get; set; }
    public AttackType? AttackType { get; set; }
    public DamageType? DamageType { get; set; }
    public float Amount { get; set; }
    public bool IsCritical { get; set; }
    public int RoundNumber { get; set; }
}
