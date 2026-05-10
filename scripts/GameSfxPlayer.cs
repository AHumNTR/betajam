using Godot;
using System;

public partial class GameSfxPlayer : Node3D
{
	private static GameSfxPlayer _instance;
	public static GameSfxPlayer Instance => _instance;

	[Export] public AudioStreamPlayer3D winSound;
	[Export] public AudioStreamPlayer3D loseSound;
	[Export] public AudioStreamPlayer3D eatSound;
	[Export] public AudioStreamPlayer3D vomitSound;

	public override void _Ready()
	{
		_instance = this;
	}

}
