using Godot;
using System;

public partial class AutoDeleteParticle : GpuParticles3D
{
	private float _lifetime;
	
	public override void _Ready()
	{
		_lifetime = 0;
	}

	public override void _Process(double delta)
	{
		_lifetime += (float)delta;
		
		if (_lifetime > 1) {
			QueueFree();
		}
	}
}
