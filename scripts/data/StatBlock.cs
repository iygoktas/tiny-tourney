using Godot;

namespace TinyTourney.Data;

[GlobalClass]
public partial class StatBlock : Resource
{
	[Export] public int Str { get; set; }
	[Export] public int Spd { get; set; }
	[Export] public int Dur { get; set; }
	[Export] public int Dex { get; set; }
	[Export] public int Luk { get; set; }
	[Export] public int Int { get; set; }

	public StatBlock Clone()
	{
		return new StatBlock
		{
			Str = Str,
			Spd = Spd,
			Dur = Dur,
			Dex = Dex,
			Luk = Luk,
			Int = Int
		};
	}

	public void Add(StatBlock other)
	{
		Str += other.Str;
		Spd += other.Spd;
		Dur += other.Dur;
		Dex += other.Dex;
		Luk += other.Luk;
		Int += other.Int;
	}
}
