using Godot;
using System;
using System.Diagnostics;

public partial class DoorControls : Control
{
	private BaseGenerator baseGen;
	[Export] public Node container;
	[Export] public ColorRect player;
	[Export] public float gridSize, wallThickness;
	[Export] public PackedScene roomUI, doorUI;
	public Vector2 offset;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		baseGen = BaseGenerator.Instance;
		baseGen.afterGenerated += GenerateUI;
	}

	public void GenerateUI() {
		foreach (var child in container.GetChildren()) {
			child.QueueFree();
		}

		foreach ((Vector3 wall, bool rot) in baseGen.walls) {
			Panel cell = new Panel();
			if (rot) {
				cell.Size = new Vector2(gridSize * 2f, wallThickness);
				cell.Position = new Vector2(wall.X * gridSize + 250f - gridSize, wall.Z * gridSize + 250f);
			} else {
				cell.Size = new Vector2(wallThickness, gridSize * 2f);
				cell.Position = new Vector2(wall.X * gridSize + 250f, wall.Z * gridSize + 250f - gridSize);
			}
			cell.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(1.0f, 1.0f, 1.0f, 1) });
			container.AddChild(cell);
		}

		foreach ((Vector3 pos, Door door) in baseGen.doors) {
			Panel cell = doorUI.Instantiate<Panel>();
			DoorUI script = cell as DoorUI;
			script.door = door;

			if (door.rotated) {
				cell.Size = new Vector2(gridSize * 2f, wallThickness);
				cell.Position = new Vector2(pos.X * gridSize + 250f - gridSize, pos.Z * gridSize + 250f);
			} else {
				cell.Size = new Vector2(wallThickness, gridSize * 2f);
				cell.Position = new Vector2(pos.X * gridSize + 250f, pos.Z * gridSize + 250f - gridSize);
			}
			cell.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.2f, 0.8f, 0.2f, door.open ? 0.2f : 1f ) });
			container.AddChild(cell);
		}

		foreach ((Vector3 pos, Room room) in baseGen.rooms) {
			Panel cell = roomUI.Instantiate<Panel>();
			RoomUI script = cell as RoomUI;
			script.room = room;

			cell.Size = new Vector2(gridSize * 2f, gridSize * 2f);
			cell.Position = new Vector2(pos.X * gridSize + 250f - gridSize, pos.Z * gridSize + 250f - gridSize);
			cell.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.2f, 0.2f, 0.8f, 0.25f ) });
			container.AddChild(cell);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Player.Instance != null) {
			var pos = Player.Instance.Position;
			player.Position = new Vector2(pos.X * gridSize + 250f, pos.Z * gridSize + 250f) - offset;
		}

		((Control)container).Position = -offset;

		if (Input.IsActionJustPressed("ui-left")) {
			MoveOffset(Vector2.Left);
		}

		if (Input.IsActionJustPressed("ui-up")) {
			MoveOffset(Vector2.Up);
		}

		if (Input.IsActionJustPressed("ui-down")) {
			MoveOffset(Vector2.Down);
		}

		if (Input.IsActionJustPressed("ui-right")) {
			MoveOffset(Vector2.Right);
		}

		if (Input.IsActionJustPressed("ui-enter")) {
			var point = new Vector3I(Mathf.RoundToInt(offset.X / gridSize), 0, Mathf.RoundToInt(offset.Y / gridSize));
			baseGen.doors[point].open = !baseGen.doors[point].open;
		}
	}

	public void MoveOffset(Vector2 dir) {
		var newPos = offset + dir * gridSize;
		var point = new Vector3I(Mathf.RoundToInt(newPos.X / gridSize), 0, Mathf.RoundToInt(newPos.Y / gridSize));

		if (baseGen.doors.ContainsKey(point) || baseGen.rooms.ContainsKey(point) 
			 || baseGen.walls.Contains((point, true)) || baseGen.walls.Contains((point, false))) {
			offset = newPos;
			return;
		}

		newPos = newPos + dir * gridSize;
		point = new Vector3I(Mathf.RoundToInt(newPos.X / gridSize), 0, Mathf.RoundToInt(newPos.Y / gridSize));

		if (baseGen.doors.ContainsKey(point) || baseGen.walls.Contains((point, true)) || baseGen.walls.Contains((point, false))) {
			offset = newPos;
		}
	}
}
