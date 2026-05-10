using Godot;
using System;

public partial class TimerManager : Node3D
{
	private static TimerManager _instance;
	public static TimerManager Instance => _instance;

	[Export] public RichTextLabel RemainingTimeLabel;
	[Export] public ColorRect TransitionRect;
	public bool TimerRunsDown;
	private AudioStreamPlayer3D audioPlayer;
	private float _timeSpeed = 1f;
	private float _remainingTime;
	private const float totalTime = 180f;
	public override void _Ready()
	{
		_instance = this;

		_remainingTime = totalTime;
		audioPlayer = (AudioStreamPlayer3D)GetNode("AudioStreamPlayer3D");

		TimerRunsDown = true;
	}

	public override void _Process(double delta)
	{
		if (!TimerRunsDown) 
		{
			if (TransitionRect.Color.A <= 1f)
			{
				TransitionRect.Color += new Color(0, 0, 0, 1) * (float)delta;
			}
			return;
		}

		_remainingTime -= (float)delta * _timeSpeed;

		if (_remainingTime <= 0)
		{
			TimerRunsDown = false;
			GameSfxPlayer.Instance.loseSound.Play();
			GameFinishTask("res://scenes/MainMenu.tscn");
			return;
		}

		if (_remainingTime <= 60)
		{
			RemainingTimeLabel.Visible = true;
			RemainingTimeLabel.Text = $"{_remainingTime / _timeSpeed:F1}";
		}
	}

	public void SpeedUp()
	{
		_timeSpeed *= 1.2f;
		audioPlayer.PitchScale = _timeSpeed;
	}

	public void ResetTimer()
	{
		_timeSpeed = 1;
		_remainingTime = totalTime;
		audioPlayer.PitchScale = 1;
		audioPlayer.Play();
	}

	public async void GameFinishTask(string nextScene)
	{
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		GetTree().ChangeSceneToFile(nextScene);
	}
}
