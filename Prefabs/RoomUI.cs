using Godot;
using System;

public partial class RoomUI : Panel
{
	public Room room;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.2f, 0.2f, 0.8f, room.flooded ? 0.5f : 0f ) });
	}
}
