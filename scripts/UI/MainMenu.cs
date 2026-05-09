using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	Random r;

	public override void _Ready()
	{
		
		r=new Random(0);
		Seed.seed=r.Next();
		GetNode<LineEdit>("SeedEdit").Text=Seed.seed.ToString();
	}
	public void _on_start_button_pressed(){
		
		GetTree().ChangeSceneToFile("res://scenes/main_scene.tscn");
	}
	public void _on_seed_edit_text_changed(string newText){
		Seed.seed=newText.ToInt();
	}
	

}
