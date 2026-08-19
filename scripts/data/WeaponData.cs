using Godot;

namespace TinyTourney.Data;

[GlobalClass]
public partial class WeaponData : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string DisplayName { get; set; }
	[Export] public int Tier { get; set; }
	[Export] public int MinLevel { get; set; }
	[Export] public float NormalDamage { get; set; }
	[Export] public float ThrustDamage { get; set; }
	[Export] public float CritChance { get; set; }
	[Export] public float CritMultiplier { get; set; }
	[Export] public float StrScaling { get; set; }
	[Export] public string IconPath { get; set; }
}
