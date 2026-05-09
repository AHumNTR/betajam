using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Sensitivity = 0.002f;
    private float yaw = 0f;
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

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
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
