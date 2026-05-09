using Godot;
using System;
using System.Collections.Generic;

public partial class Spawner : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public PackedScene[] singleObjects;
	[Export]
	public PackedScene[] objectiveScenes;
	[Export]
	public BaseMaterial3D.BillboardModeEnum bilboard;
	[Export] public Material PathDitherMaterial;
	public override void _Ready()
	{
		Map m = Map.CreateMap(Seed.seed);
		foreach (Map.SingleObject o in m.SingleObjects)
		{
			Node3D obj = (Node3D)singleObjects[o.ObjectType].Instantiate();
			obj.GetNode<Sprite3D>("Sprite3D").Billboard = bilboard;
			this.AddChild(obj);
			obj.Position = new Vector3(o.Position.X, 0, o.Position.Y);

		}
		foreach (Map.Objective o in m.Objectives)
		{
			Node3D obj = (Node3D)singleObjects[o.ObjectType].Instantiate();
			this.AddChild(obj);
			obj.Position = new Vector3(o.Position.X, 0, o.Position.Y);
		}

		CreatePaths(m.SafeLines);
	}

	public void CreatePaths(List<Map.SafeLine> lines)
	{
		SurfaceTool st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		foreach (var line in lines)
		{
			Vector3 start = new Vector3(line.Start.X, 0, line.Start.Y);
			Vector3 end = new Vector3(line.End.X, 0, line.End.Y);

			Vector3 offset = new(0, -0.95f, 0);
			const float width = 2f;

			start += offset;
			end += offset;

			Vector3 dir = (end - start).Normalized();
			Vector3 normal = new Vector3(-dir.Z, 0, dir.X) * width;

			var startLeft = start - normal;
			var startRight = start + normal;
			var endLeft = end - normal;
			var endRight = end + normal;

			var colorOpaque = new Color(1, 1, 1, 1);
			var colorTransparent = new Color(1, 1, 1, 0);

			int[] vertexArray = [	2, 4, 0, 4, 1, 0, 
									0, 1, 3, 1, 5, 3,
									4, 6, 1, 6, 7, 1, 1, 7, 5,
									2, 0, 8, 8, 0, 9, 0, 3, 9];

			foreach (var v in vertexArray)
			{
				if (v <= 1)
				{
					st.SetColor(colorOpaque);
				}
				else
				{
					st.SetColor(colorTransparent);
				}

				Vector3 vertex = v switch
				{
					0 => start,
					1 => end,
					2 => startLeft,
					3 => startRight,
					4 => endLeft,
					5 => endRight,
					6 => 0.5f * (endLeft + end) + dir,	
					7 => 0.5f * (endRight + end) + dir,	
					8 => 0.5f * (startLeft + start) - dir,	
					9 => 0.5f * (startRight + start) - dir,	
					_ => Vector3.Zero,
				};
				st.AddVertex(vertex);
			}

		}

		ArrayMesh mesh = st.Commit();

		MeshInstance3D meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;
		meshInstance.SetSurfaceOverrideMaterial(0, PathDitherMaterial);

		AddChild(meshInstance);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
