using Godot;
using System;

public partial class TutorialLabelAutoFadeOut : Label
{
	[Export] public Color MainColor = new(1, 1, 1, 1);
	[Export] public float LifeTime = 5f;
	private ulong _startTime;

	public override void _Ready()
	{
		_startTime = Time.GetTicksMsec();
	}

	public override void _Process(double delta)
	{
		var elapsedTime = (Time.GetTicksMsec() - _startTime) / 1000.0f;
		var tr = Mathf.Clamp(elapsedTime / LifeTime, 0, 1);
		var timeRatioEased = tr * tr * tr * tr * tr;
		var newColor = MainColor;
		newColor.A = 1 - timeRatioEased;

		AddThemeColorOverride("font_color", newColor);

		if (timeRatioEased >= 1)
		{
			QueueFree();
		}
	}
}
