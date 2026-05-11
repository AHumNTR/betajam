using Godot;
using System;

public partial class LoadingIcon : TextureRect
{
	public override void _Process(double delta)
	{
		RotationDegrees += 360 * (float)delta;
	}
}
