using Godot;
using System;

public partial class MushroomLabel : RichTextLabel
{
	public void UpdateText()
	{
		Text = $"{End.MaxRemainingItems - End.RemainingItems}/{End.MaxRemainingItems}";
	}
}
