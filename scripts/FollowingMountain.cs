using Godot;
using System;

public partial class FollowingMountain : Node3D
{
	[Export] Player player;

	public override void _Process(double delta)
	{
		if (player != null)
		{
			GlobalPosition = player.GlobalPosition;
		}
	}
}
