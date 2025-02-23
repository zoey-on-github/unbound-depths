using Godot;
using System;

public partial class Door : Node3D
{
	[Export] public bool open, broken;
	[Export] public MeshInstance3D renderer;
	[Export] public CollisionShape3D collider;
	[Export] public float durability = 10f;
	public bool rotated;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		renderer.Visible = !open;
		collider.Disabled = open;
	}

	public void Break() {
		broken = true;
		open = true;
	}
}
