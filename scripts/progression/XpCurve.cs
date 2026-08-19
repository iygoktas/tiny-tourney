namespace TinyTourney.Progression;

public static class XpCurve
{
    public static int XpRequiredForLevel(int level) => 10 + level * 5 + level * level;
}
