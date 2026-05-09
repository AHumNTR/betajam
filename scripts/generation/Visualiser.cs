using Godot;
using System;

public partial class Visualiser : MeshInstance3D
{
	// Called when the node enters the scene tree for the first time.
	Map m;
	public override void _Ready()
	{
		m = Map.CreateMap(0);
		var immediateMesh = new ImmediateMesh();
		this.Mesh = immediateMesh;

		float thickness = 0.5f; // Adjust this for your desired width
		var material = new StandardMaterial3D();
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		this.MaterialOverride = material;

		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

		foreach (Map.SafeLine s in m.SafeLines)
		{
		Vector3 start = new Vector3(s.Start.X, s.Start.Y, 0);
		Vector3 end = new Vector3(s.End.X, s.End.Y, 0);
		Vector3 dir = (end - start).Normalized();
		Vector3 normal = new Vector3(-dir.Y, dir.X, 0) * (thickness / 2.0f);
		Vector3 A = start + normal;
		Vector3 B = start - normal;
		Vector3 C = end + normal;
		Vector3 D = end - normal;
		immediateMesh.SurfaceAddVertex(A);
		immediateMesh.SurfaceAddVertex(B);
		immediateMesh.SurfaceAddVertex(C);
		immediateMesh.SurfaceAddVertex(B);
		immediateMesh.SurfaceAddVertex(D);
		immediateMesh.SurfaceAddVertex(C);
		}

		immediateMesh.SurfaceEnd();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
