using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public static Player Instance;
	[Export] public float Speed = 5.0f;
	[Export] public float decel, accel = 1f;
	[Export] public Camera3D camera;
	[Export] public Vector2 mouseSens;
	[Export] public Vector2 pitchClamp;
	public float JumpVelocity = 4.5f;
	public bool inMenu;

    public override void _Ready()
    {
		Instance = this;
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		/*
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}
*/ //i dont think we/re going to need jumping at all for this game

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction.DistanceTo(Vector3.Zero) > 0.1f && !inMenu) 
		{
			velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, accel * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * Speed, accel * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, decel * (float)delta);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, decel * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseButton && !inMenu) {
			var mouseVelocity = eventMouseButton.Relative;
			RotateY(mouseVelocity.X * mouseSens.X);
			var pitch = Mathf.Clamp(camera.Rotation.X + (mouseVelocity.Y * mouseSens.Y), pitchClamp.X, pitchClamp.Y);
			camera.Rotation = new Vector3(pitch, 0, 0);
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if (keyEvent.Keycode == Key.Escape) {
				Input.MouseMode = Input.MouseModeEnum.Visible;
            }

			if (keyEvent.Keycode == Key.Q) {
				inMenu = !inMenu;
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && !inMenu) 
            {
				Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
	}
}
