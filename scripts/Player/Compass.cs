using Godot;
using System;

public partial class Compass : Node3D
{
	// Called when the node enters the scene tree for the first time.
	Vector3 rot=new Vector3(10000,0,0);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		LookAt(rot);
	}
}
