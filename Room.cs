using Godot;
using System;

public partial class Room : Node3D
{
	[Export] public bool flooded;
	[Export] public MeshInstance3D water; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		water.Visible = flooded;
	}
}
