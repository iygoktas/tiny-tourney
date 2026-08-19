using System.Collections.Generic;
using Godot;

namespace TinyTourney.UI;

/// <summary>
/// Spawns the damage numbers and Miss / Blocked / Counter words that rise and fade over a
/// fighter. Labels are built in code and reused, so the battle scene needs no extra nodes
/// and nothing is allocated per hit once the pool has warmed up.
///
/// This is deliberately not a <see cref="TinyTourney.Core.NodePool{T}"/>: that pool
/// instantiates from a PackedScene, and these labels have no scene file.
/// </summary>
public sealed class FloatingCombatText
{
	private const float RiseHeight = 46f;
	private const float DriftSpread = 26f;
	private const float LifeSeconds = 0.85f;

	private readonly Control _parent;
	private readonly Stack<Label> _idle = new();
	private readonly RandomNumberGenerator _rng = new();

	public FloatingCombatText(Control parent, int prewarmCount = 6)
	{
		_parent = parent;
		_rng.Randomize();

		for (int i = 0; i < prewarmCount; i++)
		{
			_idle.Push(CreateLabel());
		}
	}

	/// <summary>
	/// Shows one piece of combat text at <paramref name="anchor"/> (a position in the parent's
	/// coordinates, normally the centre of the fighter that was hit).
	/// </summary>
	public void Show(string text, Color color, Vector2 anchor, int fontSize, float speedMultiplier)
	{
		Label label = _idle.Count > 0 ? _idle.Pop() : CreateLabel();

		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.AddThemeColorOverride("font_color", color);
		label.Modulate = Colors.White;
		label.Visible = true;

		// Size is only correct after the theme override is applied, so centre using the
		// freshly measured minimum size rather than the stale one.
		Vector2 measured = label.GetMinimumSize();
		float drift = _rng.RandfRange(-DriftSpread, DriftSpread);
		var start = new Vector2(anchor.X - measured.X * 0.5f + drift, anchor.Y);
		label.Position = start;

		float life = Mathf.Max(0.05f, LifeSeconds / speedMultiplier);

		var tween = label.CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(label, "position", start + new Vector2(drift * 0.4f, -RiseHeight), life)
			 .SetEase(Tween.EaseType.Out)
			 .SetTrans(Tween.TransitionType.Cubic);
		// Hold the text solid for the first part of its life, then fade.
		tween.TweenProperty(label, "modulate:a", 0f, life * 0.45f)
			 .SetDelay(life * 0.55f);

		tween.Chain().TweenCallback(Callable.From(() => Recycle(label)));
	}

	private void Recycle(Label label)
	{
		label.Visible = false;
		_idle.Push(label);
	}

	private Label CreateLabel()
	{
		var label = new Label
		{
			Visible = false,
			// Text must never intercept clicks on the buttons underneath it.
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ZIndex = 100
		};

		// A dark outline keeps the numbers readable over any arena background.
		label.AddThemeConstantOverride("outline_size", 6);
		label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.85f));

		_parent.AddChild(label);
		return label;
	}
}
