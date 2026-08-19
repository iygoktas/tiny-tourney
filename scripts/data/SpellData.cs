using Godot;

namespace TinyTourney.Data;

[GlobalClass]
public partial class SpellData : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string DisplayName { get; set; }
	[Export] public int Tier { get; set; }
	[Export] public int MinLevel { get; set; }
	[Export] public DamageType DamageType { get; set; }
	[Export] public float BaseDamage { get; set; }
	[Export] public int ManaCost { get; set; }
	[Export] public float Cooldown { get; set; }
	[Export] public string IconPath { get; set; }
	[Export] public string VfxPath { get; set; }
}
