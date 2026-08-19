using TinyTourney.Combat;

namespace TinyTourney.UI;

public static class BattleContext
{
    public static CombatantState PlayerState { get; set; }
    public static CombatantState EnemyState { get; set; }
    public static bool IsBoss { get; set; }
}
