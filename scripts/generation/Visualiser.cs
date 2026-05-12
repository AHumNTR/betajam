using Godot;

[Tool] // This allows the script to run in the editor
public partial class Visualiser : MeshInstance3D
{
	private Map m;

	[Export]
	public float Thickness { get; set; } = 0.5f;

	[Export]
	public int seed  ;
	[Export]
	public bool GenerateMap
	{
		get => false;
		set
		{
			if (value) 
			{
				CreateNewMap();
			}
		}
	}
	[Export] public bool Disabled = true;

	public override void _Ready()
	{
		if (Disabled) return;

		// Generate once on startup
		CreateNewMap();
	}

	public void CreateNewMap()
	{
		// 1. Create/Refresh Map Data
		// Assuming Map.CreateMap(0) handles its own randomness or logic
		m = Map.CreateMap(seed);

		var immediateMesh = new ImmediateMesh();
		this.Mesh = immediateMesh;

		// 2. Setup Material
		var material = new StandardMaterial3D();
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = new Color(1, 1, 1);
		this.MaterialOverride = material;

		// 3. Build Geometry
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

		foreach (Map.SafeLine s in m.SafeLines)
		{
			Vector3 start = new Vector3(s.Start.X, s.Start.Y, 0);
			Vector3 end = new Vector3(s.End.X, s.End.Y, 0);

			Vector3 dir = (end - start).Normalized();
			Vector3 normal = new Vector3(-dir.Y, dir.X, 0) * (Thickness / 2.0f);

			Vector3 v0 = start + normal;
			Vector3 v1 = start - normal;
			Vector3 v2 = end + normal;
			Vector3 v3 = end - normal;

			// First Triangle
			immediateMesh.SurfaceAddVertex(v0);
			immediateMesh.SurfaceAddVertex(v1);
			immediateMesh.SurfaceAddVertex(v2);

			// Second Triangle
			immediateMesh.SurfaceAddVertex(v1);
			immediateMesh.SurfaceAddVertex(v3);
			immediateMesh.SurfaceAddVertex(v2);
		}

		// objects
		immediateMesh.SurfaceSetColor(new Color(0xFFFF00FF));
		foreach (var singleObj in m.SingleObjects)
		{
			var v0 = new Vector3(singleObj.Position.X, singleObj.Position.Y + 1f, 0);
			var v1 = new Vector3(singleObj.Position.X - 1f, singleObj.Position.Y, 0);
			var v2 = new Vector3(singleObj.Position.X + 1f, singleObj.Position.Y, 0);
			immediateMesh.SurfaceAddVertex(v0);
			immediateMesh.SurfaceAddVertex(v1);
			immediateMesh.SurfaceAddVertex(v2);
		}

		immediateMesh.SurfaceEnd();
		
		GD.Print("Map Generated!");
	}
}
