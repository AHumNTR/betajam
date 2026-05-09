using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float Sensitivity = 0.002f;
	[Export] public float HeadBobVertical = 0.12f;
	[Export] public float HeadBobHorizontal = 0.02f;

	private float yaw = 0f;
	private float _headBobCycleValue = 0f;
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public Node3D cam;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		cam = (Node3D)GetNode("Camera3D");
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			// Horizontal rotation: Rotate the Player/Body around the Y-axis
			RotateY(-mouseMotion.Relative.X * Sensitivity);

			// Vertical rotation: Rotate the Camera around the X-axis
			yaw -= mouseMotion.Relative.Y * Sensitivity;
			yaw = Mathf.Clamp(yaw, Mathf.DegToRad(-90), Mathf.DegToRad(90));

			cam.Rotation = new Vector3(yaw, cam.Rotation.Y, cam.Rotation.Z);
		}
		if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Head Bob Cycle
		var speed = Velocity.Length();
		if (speed > 0.01f)
		{
			_headBobCycleValue += speed * (float)delta / Speed;
			if (_headBobCycleValue > 1)
			{
				_headBobCycleValue -= 1f;
			}
		}
		else if (_headBobCycleValue > 0)
		{
			if (_headBobCycleValue < 0.5f)
			{
				_headBobCycleValue -= (float)delta;
				if (_headBobCycleValue < 0)
				{
					_headBobCycleValue = 0;
				}

			}
			else
			{
				_headBobCycleValue += (float)delta;
				if (_headBobCycleValue > 1)
				{
					_headBobCycleValue = 0;
				}
			}
		}

		cam.Position = new Vector3(Mathf.Sin((_headBobCycleValue * 2f) * Mathf.Pi * 2) * HeadBobHorizontal, Mathf.Sin(_headBobCycleValue * Mathf.Pi * 2) * HeadBobVertical, 0);
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		/* if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		} */

		var speedMultiplier = 1f;
		if (Input.IsActionPressed("sprint"))
		{
			speedMultiplier = 2f;
		}

		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed * speedMultiplier;
			velocity.Z = direction.Z * Speed * speedMultiplier;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
