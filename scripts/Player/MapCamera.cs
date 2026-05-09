using Godot;
using System;

public partial class MapCamera : Camera3D
{
    [ExportGroup("Movement Settings")]
    [Export] public float DragSensitivity = 0.05f;
    [Export] public Vector2 MovementLimits = new Vector2(50, 50);

    [ExportGroup("Size (Zoom) Settings")]
    [Export] public float ZoomSpeed = 10.0f;
    [Export] public float MinSize = 2.0f;
    [Export] public float MaxSize = 20.0f;

    private bool _isDragging = false;

    public override void _UnhandledInput(InputEvent @event)
    {
        // Check for Mouse Button (Left Click to drag)
        if (@event is InputEventMouseButton mouseBtn)
        {
            if (mouseBtn.ButtonIndex == MouseButton.Left)
            {
                _isDragging = mouseBtn.Pressed;
            }
        }

        if (_isDragging && @event is InputEventMouseMotion mouseMotion)
        {
            float zoomFactor = Size / MaxSize;
            Vector3 newPos = Position;
            newPos.X -= mouseMotion.Relative.X * DragSensitivity * zoomFactor;
            newPos.Z -= mouseMotion.Relative.Y * DragSensitivity * zoomFactor;
            newPos.X = Mathf.Clamp(newPos.X, -MovementLimits.X, MovementLimits.X);
            newPos.Z = Mathf.Clamp(newPos.Z, -MovementLimits.Y, MovementLimits.Y);
            Position = newPos;
        }
	if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                Size -= ZoomSpeed;
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                Size += ZoomSpeed;
            }

            Size = Mathf.Clamp(Size, MinSize, MaxSize);
        }
	if(@event is InputEventMagnifyGesture magnifyGesture){
		Size*=magnifyGesture.Factor;
	}
    }


   
}
