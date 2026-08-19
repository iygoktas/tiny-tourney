using Godot;

namespace TinyTourney.Data;

[GlobalClass]
public partial class RaceData : Resource
{
	[Export] public string Id { get; set; }
	[Export] public string DisplayName { get; set; }
	[Export] public StatBlock BaseStats { get; set; }
	[Export] public string VisualIdentityNote { get; set; }
	[Export] public string ReferenceImagePath { get; set; }
}
