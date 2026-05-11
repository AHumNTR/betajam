using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] public RichTextLabel RoleDescriptionLabel;
	[Export] public Control LoadingScreen;
	private bool _isLoadingAScene = false;
	private string _loadingScenePath = "";

	public override void _Ready()
	{
		var random = new RandomNumberGenerator();
		Seed.seed = random.RandiRange(10000, 99999);
		GetNode<LineEdit>("SeedEdit").Text = Seed.seed.ToString();

		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

    public override void _Process(double delta)
	{
		if (_isLoadingAScene)
		{
			var loadStatus = ResourceLoader.LoadThreadedGetStatus(_loadingScenePath);
			if (loadStatus == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				var loadedScene = (PackedScene)ResourceLoader.LoadThreadedGet(_loadingScenePath);
				GetTree().ChangeSceneToPacked(loadedScene);
			}
		}
	}

	private void LoadSceneAsync(string scenePath)
	{
		if (_isLoadingAScene) return;

		LoadingScreen.Visible = true;

		_isLoadingAScene = true;
		_loadingScenePath = scenePath;
		ResourceLoader.LoadThreadedRequest(scenePath);
	}

	public void _on_start_button_pressed()
	{
		End.RemainingItems = End.MaxRemainingItems;
		LoadSceneAsync("res://scenes/main_scene.tscn");
	}

	public void _on_map_button_pressed()
	{
		LoadSceneAsync("res://scenes/MapScene.tscn");
	}

	public void _on_seed_edit_text_changed(string newText)
	{
		Seed.seed = newText.ToInt();
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

	public void _on_seed_edit_mouse_entered()
	{
		RoleDescriptionLabel.Text = "Both parties have to enter the same seed to be able to play.";
	}

	public void show_credits()
	{
		int randomSelection = new RandomNumberGenerator().RandiRange(0, 1);
		string[] developers = ["Kürşat Kuyumcu", "Kerem Küpeli"];
		string[] artists = ["Firdevs Akoğ", "Beyza Büyük"];
		RoleDescriptionLabel.Text = $"Made in 36 Hours\n\nDevelopers: {developers[randomSelection]}, {developers[1 - randomSelection]}\n" +
														   $"Artists: {artists[randomSelection]}, {artists[1 - randomSelection]}";
	}

}
