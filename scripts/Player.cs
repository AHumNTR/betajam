using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
	[Export] public float Sensitivity = 0.002f;
	[Export] public float HeadBobVertical = 0.12f;
	[Export] public float HeadBobHorizontal = 0.02f;
	[Export] public TextureRect Crosshair;
	[Export] public AudioStream[] StepSoundsGrass;
	[Export] public AudioStream[] StepSoundsDirt;
	[Export] public PackedScene ParticlesMushroom;
	[Export] public PackedScene ParticlesVomit;

	private float yaw = 0f;
	private float _headBobCycleValue = 0f;
	private float _stepSoundPlayCounter = 0f;
	private List<Map.SafeLine> _dirtPaths;
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public Node3D cam;
	public AudioStreamPlayer3D audioPlayer;

	private Action _onAllMushroomsCollected;
	private Action _onSingleMushroomCollected;

	public void Init(List<Map.SafeLine> dirtPaths, Action onAllMushroomsCollected, Action onSingleMushroomCollected)
	{
		_dirtPaths = dirtPaths;
		_onAllMushroomsCollected = onAllMushroomsCollected;
		_onSingleMushroomCollected = onSingleMushroomCollected;

		var random = new RandomNumberGenerator();
		var selectedPath = dirtPaths[random.RandiRange(0, dirtPaths.Count - 1)];
		var centerPosition = 0.5f * (selectedPath.Start + selectedPath.End);
		GlobalPosition = new Vector3(centerPosition.X, 0.1f, centerPosition.Y);
	}

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		cam = (Node3D)GetNode("Camera3D");
		audioPlayer = (AudioStreamPlayer3D)GetNode("AudioStreamPlayer3D");
	}

	public override void _Input(InputEvent @event)
	{
		if (!TimerManager.Instance.TimerRunsDown) return;


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
		if (@event.IsActionPressed("Interact"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Head Bob Cycle
		var speed = Velocity.Length();
		if (!TimerManager.Instance.TimerRunsDown) speed = 0;
		var headBobDelta = speed * (float)delta / Speed;
		if (speed > 0.01f)
		{
			_headBobCycleValue += headBobDelta;
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

		_stepSoundPlayCounter += headBobDelta;
		if (_stepSoundPlayCounter > 0.5f)
		{
			_stepSoundPlayCounter = 0f;
			PlayStepSound();
		}

		cam.Position = new Vector3(Mathf.Sin((_headBobCycleValue * 2f) * Mathf.Pi * 2) * HeadBobHorizontal, Mathf.Sin(_headBobCycleValue * Mathf.Pi * 2) * HeadBobVertical, 0);

		if (!TimerManager.Instance.TimerRunsDown) return;

		//check for collecting
		var spaceState = GetWorld3D().DirectSpaceState;
		Vector3 forward = -cam.GlobalTransform.Basis.Z;
		Vector3 end = cam.GlobalPosition + (forward * 5.0f);	// 5f is the hardcoded distance

		var query = PhysicsRayQueryParameters3D.Create(cam.GlobalPosition, end, 2);
		var result = spaceState.IntersectRay(query);
		if (result.Count > 0)
		{
			Crosshair.Visible = true;

			if (Input.IsActionJustPressed("Interact"))
			{
				var selectedParticle = ParticlesVomit;

				Objective o = (Objective)result["collider"];
				if (o.harmless)
				{
					TimerManager.Instance.ResetTimer();
					End.RemainingItems--;

					GameSfxPlayer.Instance.eatSound.Play();
					_onSingleMushroomCollected?.Invoke();

					selectedParticle = ParticlesMushroom;

					if (End.RemainingItems <= 0)
					{
						_onAllMushroomsCollected?.Invoke();
					}
				}
				else
				{

					GameSfxPlayer.Instance.vomitSound.Play();
					TimerManager.Instance.SpeedUp();
				}

				var particles = (Node3D)selectedParticle.Instantiate();
				GetTree().CurrentScene.AddChild(particles);
				particles.GlobalPosition = (Vector3)result["position"];

				o.QueueFree();
			}
		} else
		{
			Crosshair.Visible = false;
		}

	}

	public override void _PhysicsProcess(double delta)
	{
		if (!TimerManager.Instance.TimerRunsDown) return;

		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

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

		// Teleport player back to keep in confines
		if (GlobalPosition.X > Map.MAP_SIZE / 2)
		{
			GlobalPosition -= new Vector3(Map.MAP_SIZE, 0, 0);
		}
		else if (GlobalPosition.X < -Map.MAP_SIZE / 2)
		{
			GlobalPosition += new Vector3(Map.MAP_SIZE, 0, 0);
		}

		if (GlobalPosition.Z > Map.MAP_SIZE / 2)
		{
			GlobalPosition -= new Vector3(0, 0, Map.MAP_SIZE);
		}
		else if (GlobalPosition.Z < -Map.MAP_SIZE / 2)
		{
			GlobalPosition += new Vector3(0, 0, Map.MAP_SIZE);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void PlayStepSound()
	{
		var selectedSoundPool = IsOnDirtPath() ? StepSoundsDirt : StepSoundsGrass;

		var random = new RandomNumberGenerator();
		var selectedSoundIndex = random.RandiRange(0, selectedSoundPool.Length - 1);

		audioPlayer.Stream = selectedSoundPool[selectedSoundIndex];
		audioPlayer.Play();
	}


	private bool IsOnDirtPath()
	{
		var pos = new Vector2(GlobalPosition.X, -GlobalPosition.Z);

		if (pos.X < 0) pos += new Vector2(Map.MAP_SIZE, 0);
		if (pos.Y < 0) pos += new Vector2(0, Map.MAP_SIZE);
		foreach (var path in _dirtPaths)
		{
			if (path.Overlaps(pos, 1f)) return true;
		}

		return false;
	}

	public void _on_end_body_entered(Node3D node)
	{
		if (!TimerManager.Instance.TimerRunsDown) return;

		if (node == (Node3D)this && End.RemainingItems <= 0)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GameSfxPlayer.Instance.winSound.Play();
			TimerManager.Instance.GameFinishTask("res://scenes/win_cutscene.tscn");

		}
	}
}
