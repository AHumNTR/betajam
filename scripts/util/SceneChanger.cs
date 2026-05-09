using Godot;
using System;

public partial class SceneChanger : Node3D
{
	[Export]
	string PathToScene;
	public void _change_scene(string animName){
		GetTree().ChangeSceneToFile(PathToScene);
	}
	public void _change_scene(){
		GetTree().ChangeSceneToFile(PathToScene);
	}
}
