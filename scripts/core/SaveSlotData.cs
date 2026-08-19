using System.Collections.Generic;

namespace TinyTourney.Core;

public class SaveSlotData
{
    public string RaceId { get; set; }
    public string CharacterName { get; set; }
    public int Level { get; set; } = 1;
    public int CurrentXp { get; set; }
    public RuntimeStatBlock CurrentStats { get; set; }
    public string EquippedWeaponId { get; set; }
    public string EquippedSpellId { get; set; }
    public List<string> ObtainedWeaponIds { get; set; } = new();
    public List<string> ObtainedSpellIds { get; set; } = new();
    public BattleStatistics Statistics { get; set; } = new();
    public string LastSavedUtc { get; set; }
}
