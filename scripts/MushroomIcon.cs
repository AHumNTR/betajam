using Godot;
using System;

public partial class MushroomIcon : Node3D
{
	[Export]
	public bool harmless;
	public override void _Input(InputEvent @event){
		if(!harmless)return;
		if(@event is InputEventKey a && a.KeyLabel== Key.Space){
			GetNode<AnimationPlayer>("AnimationPlayer").Play("ping");
		}
	}
}
