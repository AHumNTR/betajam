using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] public RichTextLabel RoleDescriptionLabel; 

	public override void _Ready()
	{
		var random = new RandomNumberGenerator();
		Seed.seed = random.RandiRange(10000, 99999);
		GetNode<LineEdit>("SeedEdit").Text=Seed.seed.ToString();

		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
	
	public void _on_start_button_pressed(){
		
		End.RemainingItems=End.MaxRemainingItems;
		GetTree().ChangeSceneToFile("res://scenes/main_scene.tscn");
	}
	public void _on_map_button_pressed(){
		GetTree().ChangeSceneToFile("res://scenes/MapScene.tscn");
	}
	public void _on_seed_edit_text_changed(string newText){
		Seed.seed=newText.ToInt();
	}

	public void _on_start_button_mouse_entered()
	{
		RoleDescriptionLabel.Text = "Play as a curious explorer lost in a magical forest. Try to describe your surroundings to the Guide Fairy, listen to their directions to locate the magic mushrooms. Collect 5 of them before you lose your mind in this whimsical place and find the way back home.";
	}

	public void _on_map_button_mouse_entered()
	{
		RoleDescriptionLabel.Text = "Play as a magical fairy to whispher the directions to a lost explorer. Listen to their clues and guess where they are by method of elimination. Give the right directions to lead them to the magic mushrooms and then way back home.";
	}

	public void _on_any_button_mouse_exited()
	{
		RoleDescriptionLabel.Text = "";
	}
	

}
