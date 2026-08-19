using TinyTourney.Data;

namespace TinyTourney.Progression;

public class WheelResult
{
    public WheelCategory Category { get; set; }
    public StatRollData StatRoll { get; set; }
    public WeaponData Weapon { get; set; }
    public SpellData Spell { get; set; }
    public bool IsEndgameMode { get; set; }
}
