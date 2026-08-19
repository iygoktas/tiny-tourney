using Godot;
using TinyTourney.Progression;

namespace TinyTourney.UI;

public partial class WheelController : Control
{
	[Export] public Label CategoryLabel;
	[Export] public Label ResultLabel;
	[Export] public Button ContinueButton;

	public override void _Ready()
	{
		ContinueButton.Text = TranslationServer.Translate("BTN_CONTINUE");
		ContinueButton.Pressed += OnContinuePressed;
		ShowNextResult();
	}

	private void ShowNextResult()
	{
		if (WheelContext.PendingResults.Count == 0)
		{
			GetTree().ChangeSceneToFile("res://scenes/screens/main.tscn");
			return;
		}

		var levelUp = WheelContext.PendingResults.Dequeue();
		var wheel = levelUp.WheelResult;

		CategoryLabel.Text = wheel.Category switch
		{
			WheelCategory.Weapon => TranslationServer.Translate("WHEEL_CATEGORY_WEAPON"),
			WheelCategory.Spell => TranslationServer.Translate("WHEEL_CATEGORY_SPELL"),
			_ => TranslationServer.Translate("WHEEL_CATEGORY_STAT")
		};

		ResultLabel.Text = wheel.Category switch
		{
			WheelCategory.Weapon => wheel.Weapon.DisplayName,
			WheelCategory.Spell => wheel.Spell.DisplayName,
			_ => $"{TranslationServer.Translate($"STAT_{wheel.StatRoll.Stat.ToString().ToUpper()}")} +{wheel.StatRoll.Amount}"
		};
	}

	private void OnContinuePressed()
	{
		ShowNextResult();
	}
}
