using Godot;
using System;

public partial class PingingCircle : Sprite3D
{
	
	public override void _Input(InputEvent @event){
		if(@event is InputEventKey a && a.KeyLabel== Key.Space){
			GetNode<AnimationPlayer>("AnimationPlayer").Play("Ping");
		}
	}
}
