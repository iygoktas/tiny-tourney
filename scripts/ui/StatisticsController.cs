using Godot;
using TinyTourney.Core;

namespace TinyTourney.UI;

public partial class StatisticsController : Control
{
    [Export] public Label BattlesWonLabel;
    [Export] public Label BattlesLostLabel;
    [Export] public Label BossesDefeatedLabel;
    [Export] public Label HighestLevelLabel;
    [Export] public Label TotalBattlesLabel;
    [Export] public Button BackButton;

    public override void _Ready()
    {
        var stats = GameState.Instance.Active.Statistics;
        BattlesWonLabel.Text = stats.BattlesWon.ToString();
        BattlesLostLabel.Text = stats.BattlesLost.ToString();
        BossesDefeatedLabel.Text = stats.BossesDefeated.ToString();
        HighestLevelLabel.Text = stats.HighestLevelReached.ToString();
        TotalBattlesLabel.Text = stats.TotalBattlesPlayed.ToString();

        BackButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
    }
}
