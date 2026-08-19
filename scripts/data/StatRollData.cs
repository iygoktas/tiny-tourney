using Godot;

namespace TinyTourney.Data;

[GlobalClass]
public partial class StatRollData : Resource
{
	[Export] public StatType Stat { get; set; }
	[Export] public float Amount { get; set; }
	[Export] public int Tier { get; set; }
	[Export] public int MinLevel { get; set; }
}
