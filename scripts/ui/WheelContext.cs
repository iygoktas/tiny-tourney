using System.Collections.Generic;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public static class WheelContext
{
    public static Queue<LevelUpResult> PendingResults { get; set; } = new();
}
