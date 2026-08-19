namespace TinyTourney.Core;

using TinyTourney.Data;

public class RuntimeStatBlock
{
    public int Str { get; set; }
    public int Spd { get; set; }
    public int Dur { get; set; }
    public int Dex { get; set; }
    public int Luk { get; set; }
    public int Int { get; set; }

    public RuntimeStatBlock Clone()
    {
        return new RuntimeStatBlock
        {
            Str = Str,
            Spd = Spd,
            Dur = Dur,
            Dex = Dex,
            Luk = Luk,
            Int = Int
        };
    }

    public static RuntimeStatBlock FromDesignStats(StatBlock designStats)
    {
        return new RuntimeStatBlock
        {
            Str = designStats.Str,
            Spd = designStats.Spd,
            Dur = designStats.Dur,
            Dex = designStats.Dex,
            Luk = designStats.Luk,
            Int = designStats.Int
        };
    }
}
