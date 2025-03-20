using Godot;
using Godot.Collections;
using System;
using System.Timers;
using Timer = Godot.Timer;

public partial class Player : CharacterBody3D {
    public static Player Instance;
    [Export] public float Speed = 5.0f;
    [Export] public float decel, accel = 0.5f;
    [Export] public Camera3D camera;
    [Export] public Vector2 mouseSens;
    [Export] public Vector2 pitchClamp;
    //[Export] public Timer dashTimer;
    public float JumpVelocity = 4.5f;
    public bool inMenu;

    public override void _Ready() {
        Instance = this;
    }

    public override void _PhysicsProcess(double delta) {
        Vector3 velocity = Velocity;
        GD.Print(Player.Instance.accel);

        // Add the gravity.
        if (!IsOnFloor()) {
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
        if (direction.DistanceTo(Vector3.Zero) > 0.1f && !inMenu) {
            velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, accel * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * Speed, accel * (float)delta);
        }
        else {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, decel * (float)delta);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, decel * (float)delta);
        }

        Velocity = velocity;
        MoveAndSlide();
        if (GetSlideCollisionCount() > 1) {
            GD.Print(GetSlideCollisionCount());
        }
    }
    private void AreaOnAreaEntered(Area3D area) {
        throw new NotImplementedException();
    }

    public override void _Input(InputEvent @event) {
        var playerCharacter = GetNode<CharacterBody3D>("../Player");
        var thirdPersonCamera = GetNode<Camera3D>("../Camera3D");
        if (@event is InputEventMouseMotion eventMouseButton && !inMenu) {
            if (thirdPersonCamera.Current) {
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                Vector2 mousePosition = GetViewport().GetMousePosition();
                Vector3 rayOrigin = thirdPersonCamera.ProjectRayOrigin(mousePosition);
                Vector3 rayEnd = rayOrigin + thirdPersonCamera.ProjectRayNormal(mousePosition) * 2000;
                playerCharacter.LookAt(rayEnd);
            }

            else {
                var mouseVelocity = eventMouseButton.Relative;
                RotateY(mouseVelocity.X * mouseSens.X);
                //	RotateX(mouseVelocity.Y * mouseSens.Y);
                GD.Print(playerCharacter.Rotation);
                var pitch = Mathf.Clamp(camera.Rotation.X + (mouseVelocity.Y * mouseSens.Y), pitchClamp.X, pitchClamp.Y);
                camera.Rotation = new Vector3(pitch, 0, 0);
                GD.Print(pitch);
            }
        }

        //	GD.Print(camera.Rotation);
        //	break;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if (keyEvent.Keycode == Key.Escape) {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }

            if (keyEvent.Keycode == Key.Q) {
                inMenu = !inMenu;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            if (keyEvent.Keycode == Key.Shift) {
                var dashTimer = GetNode<Timer>("Timer");
                dashTimer.Start();
                Player.Instance.accel = 3f;
                //dashTimer.Timeout -= timerFinish;

            }
        }

        void timerFinish() {
            GD.Print("time passed");
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed) {
            if (mouseButton.ButtonIndex == MouseButton.Left && !inMenu) {
                //	Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
}
