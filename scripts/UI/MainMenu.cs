using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	RandomNumberGenerator r;
	public override void _Ready()
	{
		GD.Print(Time.GetTimeDictFromSystem());
	}
	public void _on_start_button_pressed(){
		
		GD.Print("baller");
		GetTree().ChangeSceneToFile("res://scenes/main_scene.tscn");
	}
	

}
