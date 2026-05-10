using Godot;
using System;

public partial class Objective : StaticBody3D
{
	[Export]
	PackedScene pingingCircle;
	[Export]
	public bool harmless;
	public override void _Input(InputEvent @event){
		if(!harmless)return;
		if(@event is InputEventKey a && a.KeyLabel== Key.Space){
			Sprite3D x=(Sprite3D)pingingCircle.Instantiate();
			x.GlobalPosition=GlobalPosition;
			AddSibling(x);
		}
	}
	
}
