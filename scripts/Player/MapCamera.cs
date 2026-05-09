using Godot;
using System;

public partial class MapCamera : Camera3D
{
	[ExportGroup("Movement Settings")]
	[Export] public float DragSensitivity = 0.05f;
	[Export] public float MovementLimit = 250f;

	[ExportGroup("Size (Zoom) Settings")]
	[Export] public float ZoomSpeed = 10.0f;
	[Export] public float MinSize = 10.0f;
	[Export] public float MaxSize = 300.0f;

	private Vector3 _dragStartPoint;
	private bool _isDragging = false;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				_dragStartPoint = GetGroundPosition(mouseButton.Position);
				_isDragging = true;
			}
			else
			{
				_isDragging = false;
			}
		}

		if (@event is InputEventMouseMotion mouseMotion && _isDragging)
		{
			Vector3 currentGroundPoint = GetGroundPosition(mouseMotion.Position);
			Vector3 drift = currentGroundPoint - _dragStartPoint;

			// Move the camera in the opposite direction of the mouse drift
			// to keep the ground point under the cursor.
			GlobalPosition -= new Vector3(drift.X, 0, drift.Z);
		}

		var sizeRatio = Size / MaxSize;

		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
			{
				if (Size <= ZoomSpeed * sizeRatio)
				{
					Size = MinSize;
				}
				else
				{
					Size -= ZoomSpeed * sizeRatio;
				}
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				Size += ZoomSpeed * sizeRatio;
			}

		}

		if (@event is InputEventMagnifyGesture magnifyGesture)
		{
			if (Size * magnifyGesture.Factor < MinSize)
			{
				Size = MinSize;
			}
			else
			{
				Size *= magnifyGesture.Factor;
			}
		}

		Size = Mathf.Clamp(Size, MinSize, MaxSize);

		// Camera clamp
		var limitPositive = 0.5f * (MaxSize - Size);
		var limitNegative = 0.5f * (-MaxSize + Size);
		if (GlobalPosition.X > limitPositive)
		{
			GlobalPosition = new Vector3(limitPositive, GlobalPosition.Y, GlobalPosition.Z);
		}
		else if (GlobalPosition.X < limitNegative)
		{
			GlobalPosition = new Vector3(limitNegative, GlobalPosition.Y, GlobalPosition.Z);
		}

		if (GlobalPosition.Z > limitPositive)
		{
			GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y, limitPositive);
		}
		else if (GlobalPosition.Z < limitNegative)
		{
			GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y, limitNegative);
		}
	}

	private Vector3 GetGroundPosition(Vector2 mousePos)
	{
		Vector3 rayOrigin = ProjectRayOrigin(mousePos);
		Vector3 rayNormal = ProjectRayNormal(mousePos);

		Plane groundPlane = new Plane(Vector3.Up, 0);

		Vector3? intersection = groundPlane.IntersectsRay(rayOrigin, rayNormal);

		if (intersection.HasValue)
		{
			return intersection.Value;
		}

		return Vector3.Zero;
	}

}
