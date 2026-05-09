using Godot;
using System;

public partial class Spawner : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public PackedScene[] singleObjects;
	[Export]
	public PackedScene[] objectiveScenes;
	public override void _Ready()
	{
		Map m=Map.CreateMap(Seed.seed);
		foreach(Map.SingleObject o in m.SingleObjects){
			Node3D obj=(Node3D)singleObjects[o.ObjectType].Instantiate();
			this.AddChild(obj);	
			obj.Position=new Vector3(o.Position.X,0,o.Position.Y);

		}
		foreach(Map.Objective o in m.Objectives){
			Node3D obj=(Node3D)singleObjects[o.ObjectType].Instantiate();
			this.AddChild(obj);	
			obj.Position=new Vector3(o.Position.X,0,o.Position.Y);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
