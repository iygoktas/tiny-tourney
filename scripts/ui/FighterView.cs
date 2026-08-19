using System.Threading.Tasks;
using Godot;
using TinyTourney.Data;

namespace TinyTourney.UI;

/// <summary>
/// The on-screen sprite for one combatant. Attach this to a TextureRect in the battle scene.
/// There are no animation frames anywhere in the game — every bit of motion here is a tween,
/// so a single static sprite per race is all the art that is needed.
///
/// The node's own Position is written every frame from three independent offsets (idle bob,
/// attack lunge, hit shake). Nothing else should assign Position, or the effects will fight
/// each other.
/// </summary>
public partial class FighterView : TextureRect
{
	/// <summary>How far the fighter travels toward the opponent when attacking, in pixels.</summary>
	[Export] public float LungeDistance { get; set; } = 90f;

	/// <summary>Seconds for the forward half of a lunge at 1x speed.</summary>
	[Export] public float LungeOutSeconds { get; set; } = 0.16f;

	/// <summary>Seconds for the return half of a lunge at 1x speed.</summary>
	[Export] public float LungeBackSeconds { get; set; } = 0.13f;

	/// <summary>Vertical travel of the resting breathing motion, in pixels.</summary>
	[Export] public float IdleBobPixels { get; set; } = 2.5f;

	/// <summary>Full breathing cycles per second while resting.</summary>
	[Export] public float IdleBobSpeed { get; set; } = 1.4f;

	/// <summary>Starting sideways kick of the shake when this fighter is hit, in pixels.</summary>
	[Export] public float FlinchPixels { get; set; } = 7f;

	/// <summary>Seconds for the hit shake to settle at 1x speed.</summary>
	[Export] public float FlinchSeconds { get; set; } = 0.22f;

	private Vector2 _home;
	private bool _homeCaptured;

	private float _lungeOffsetX;
	private float _shakeOffsetX;
	private float _shakeRemaining;
	private float _shakeDuration;
	private float _bobPhase;

	// +1 lunges to the right, -1 to the left. Set by Setup from which side this fighter stands on.
	private float _facing = 1f;

	public override void _Ready()
	{
		// Keep the pixel art crisp no matter what the project-wide filter is set to.
		TextureFilter = TextureFilterEnum.Nearest;

		// Anchors and containers only resolve after the first layout pass, so the resting
		// position is not trustworthy yet during _Ready.
		CallDeferred(nameof(CaptureHome));
	}

	/// <summary>
	/// Points this view at a race's sprite. <paramref name="facesRight"/> is true for the
	/// fighter standing on the left of the arena.
	/// </summary>
	public void Setup(RaceData race, bool facesRight)
	{
		_facing = facesRight ? 1f : -1f;
		FlipH = !facesRight;

		if (race?.ReferenceImagePath is { Length: > 0 } path && ResourceLoader.Exists(path))
		{
			Texture = GD.Load<Texture2D>(path);
		}
		else
		{
			GD.PushWarning($"[FighterView] No sprite for race '{race?.Id ?? "null"}'; the fighter will be invisible.");
		}
	}

	/// <summary>Re-reads the resting position. Call this if the scene is resized or re-laid out.</summary>
	public void CaptureHome()
	{
		_home = Position;
		_homeCaptured = true;
	}

	/// <summary>The middle of the sprite at its resting spot, for anchoring floating text.</summary>
	public Vector2 HomeCenter => (_homeCaptured ? _home : Position) + Size * 0.5f;

	public override void _Process(double delta)
	{
		if (!_homeCaptured)
		{
			return;
		}

		_bobPhase += (float)delta * IdleBobSpeed * Mathf.Tau;
		float bobY = Mathf.Sin(_bobPhase) * IdleBobPixels;

		if (_shakeRemaining > 0f)
		{
			_shakeRemaining = Mathf.Max(0f, _shakeRemaining - (float)delta);
			// Alternate sides every frame and taper the amplitude to zero.
			float falloff = _shakeDuration > 0f ? _shakeRemaining / _shakeDuration : 0f;
			_shakeOffsetX = Mathf.Sin(_shakeRemaining * 90f) * FlinchPixels * falloff;
		}
		else
		{
			_shakeOffsetX = 0f;
		}

		Position = _home + new Vector2(_lungeOffsetX + _shakeOffsetX, bobY);
	}

	/// <summary>Steps toward the opponent. Await this, then land the hit.</summary>
	public async Task LungeOutAsync(float speedMultiplier)
	{
		float duration = Mathf.Max(0.01f, LungeOutSeconds / speedMultiplier);
		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenMethod(Callable.From<float>(SetLungeOffset), _lungeOffsetX, LungeDistance * _facing, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	/// <summary>Returns to the resting spot after a strike.</summary>
	public async Task LungeBackAsync(float speedMultiplier)
	{
		float duration = Mathf.Max(0.01f, LungeBackSeconds / speedMultiplier);
		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenMethod(Callable.From<float>(SetLungeOffset), _lungeOffsetX, 0f, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	/// <summary>Shakes and flashes bright for an instant. Fire and forget; it does not block.</summary>
	public void PlayFlinch(float speedMultiplier)
	{
		_shakeDuration = Mathf.Max(0.01f, FlinchSeconds / speedMultiplier);
		_shakeRemaining = _shakeDuration;

		// Values above 1 brighten a CanvasItem, which reads as a white hit-flash without a shader.
		Modulate = new Color(3.5f, 3.5f, 3.5f);
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", Colors.White, _shakeDuration * 0.8f);
	}

	/// <summary>Fades out and drops, for the fighter that just lost.</summary>
	public async Task PlayDefeatAsync(float speedMultiplier)
	{
		float duration = Mathf.Max(0.01f, 0.45f / speedMultiplier);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(this, "modulate:a", 0.15f, duration);
		tween.TweenProperty(this, "rotation_degrees", _facing * 75f, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	/// <summary>Puts the fighter back to an untouched resting state.</summary>
	public void ResetVisualState()
	{
		_lungeOffsetX = 0f;
		_shakeOffsetX = 0f;
		_shakeRemaining = 0f;
		Modulate = Colors.White;
		RotationDegrees = 0f;
	}

	private void SetLungeOffset(float value) => _lungeOffsetX = value;
}
