using Godot;
using System;
public partial class Node3d : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
			base._Ready();
			var camera = GetNode<Camera3D>("Camera3D");
			//camera.Position.Z = 300;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
