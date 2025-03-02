using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public struct WeightedPrefab {
    float weight;
    PackedScene prefab;
}

public partial class BaseGenerator : Node3D {
    public static BaseGenerator Instance;

    [Export] public PackedScene RoomPrefab, Wall, DoorPrefab;
    [Export] public PackedScene[] LootPrefabs;
    [Export] public float[] LootWeights;
    [Export] public int minLoot, maxLoot;
    [Export] public int Steps = 50;
    [Export] public int Walks = 3;
    [Export] public int GridSize = 2;
    [Export] public float DoorOpenLikeliness = 0.3f;
    [Export] public bool AllowVerticalMovement = false;
    [Export] public Button DebugButton, DebugFloodButton, POVButton;
    public event Action afterGenerated;
    private HashSet<Vector3I> visitedPositions = new();
    private RandomNumberGenerator rng = new();
    private Vector3I currentPosition = Vector3I.Zero;
    public Dictionary<Vector3I, Room> rooms = new();
    public Dictionary<Vector3I, Door> doors = new();
    public List<(Vector3I, bool)> walls = new();
    public HashSet<Vector3I> visitedMidpoints = new();

    public override void _Ready() {
        Instance = this;
        GenerateBase();

        DebugButton.Pressed += GenerateBase;
        DebugFloodButton.Pressed += FloodRandomRoom;
        POVButton.Pressed += test;

    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("regenerate_base")) {
            GD.Print("pressed");
            GenerateBase();
        }
        foreach (var item in rooms.ToArray()) {
            if (!item.Value.flooded) { continue; }

            var neighbours = GetNeighbours(item.Key);
            foreach (var neighbour in neighbours) {
                var midpoint = (neighbour + item.Key) / 2;
                if (rooms.ContainsKey(neighbour)) {
                    if (doors[midpoint].open) {
                        rooms[neighbour].flooded = true;
                    }
                    else {
                        if (doors[midpoint].durability > 0.1f) {
                            doors[midpoint].durability -= (float)delta;
                        }
                        else {
                            doors[midpoint].Break();
                        }
                        /*
                        if (Input.IsActionJustPressed("regenerate_base")) {
                            GenerateBase();
                        }
                        */
                    }
                }
            }
        }
    }

    public void FloodRandomRoom() {
        Debug.Print("Flooding room");
        var unflooded = rooms.Values.Where(room => !room.flooded).ToArray();
        unflooded[rng.RandiRange(0, unflooded.Length - 1)].flooded = true;
    }

    private void test() {
        var firstPersonCamera = GetNode<Camera3D>("../Player/Camera3D");
        var thirdPersonCamera = GetNode<Camera3D>("../Camera3D");
        GD.Print(firstPersonCamera.Current);
        if (thirdPersonCamera.Current) {
            thirdPersonCamera.Current = false;
            firstPersonCamera.Current = true;
        }
        else {
            thirdPersonCamera.Current = true;
            firstPersonCamera.Current = false;
        }

}
    public void GenerateBase() {
        visitedPositions = new();
        visitedMidpoints = new();
        rooms = new();
        doors = new();
        walls = new();

        foreach (Node child in GetChildren()) {
            child.QueueFree();
        }

        rng.Randomize();
        for (int i = 0; i < Walks; i++) {
            currentPosition = Vector3I.Zero;
            GenerateRandomWalk();
        }

        GenerateWalls();
        GenerateLoot();

        afterGenerated?.Invoke();
    }

    private void GenerateLoot() {
        int amount = rng.RandiRange(minLoot, maxLoot);
        for (int i = 0; i < amount; i++) {
            var room_positions = rooms.Keys.ToArray();
            var room = room_positions[rng.RandiRange(0, room_positions.Length - 1)];
            var addX = rng.RandfRange(-1f, 1f);
            var addZ = rng.RandfRange(-1f, 1f);
            var lootToSpawn = RandomLoot();
            SpawnPrefab(lootToSpawn, (Vector3)room + new Vector3(addX, 0f, addZ));
        }
    }

    private PackedScene RandomLoot() {
        var totalWeights = LootWeights.Sum();
        var n = rng.RandfRange(0f, totalWeights);

        var currentWeight = 0f;
        for (int i = 0; i < LootPrefabs.Length; i++) {
            currentWeight += LootWeights[i];
            if (n < currentWeight) {
                return LootPrefabs[i];
            }
        }

        return null;
    }

    private void GenerateWalls() {
        foreach (Vector3I room in visitedPositions) {
            var neighbours = GetNeighbours(room);

            foreach (var neighbour in neighbours) {
                var midpoint = (neighbour + room) / 2;
                if (!visitedMidpoints.Contains(midpoint)) {
                    visitedMidpoints.Add(midpoint);

                    if (!visitedPositions.Contains(neighbour)) {
                        var wall = SpawnPrefab(Wall, midpoint);
                        bool rot = false;

                        // need to rotate
                        if (Math.Abs((neighbour - room).Z) > 0) {
                            // 90 degrees
                            wall.RotateY(Mathf.Pi / 2f);
                            rot = true;
                        }

                        walls.Add((midpoint, rot));
                    }
                    else {
                        var door = SpawnPrefab(DoorPrefab, midpoint);
                        Door script = door as Door;

                        // need to rotate
                        if (Math.Abs((neighbour - room).Z) > 0) {
                            // 90 degrees
                            door.RotateY(Mathf.Pi / 2f);
                            script.rotated = true;
                        }

                        var open = rng.Randf() < DoorOpenLikeliness;
                        var durabilityAdd = rng.Randf() * 6f - 3f;
                        script.durability += durabilityAdd;
                        script.open = open;

                        doors.Add(midpoint, script);
                    }
                }
            }
        }
    }

    private Vector3I[] GetNeighbours(Vector3I center) {
        var neighbours = new Vector3I[] {
            Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back
        };

        for (int i = 0; i < neighbours.Length; i++) {
            neighbours[i] *= 2;
            neighbours[i] += center;
        }

        return neighbours;
    }

    private void GenerateRandomWalk() {
        for (int i = 0; i < Steps; i++) {
            if (!visitedPositions.Contains(currentPosition)) {
                Room room = SpawnPrefab(RoomPrefab, currentPosition) as Room;
                visitedPositions.Add(currentPosition);
                rooms.Add(currentPosition, room);
            }

            currentPosition += GetRandomDirection();
        }
    }

    private Node3D SpawnPrefab(PackedScene Prefab, Vector3 position) {
        if (Prefab != null) {
            Node3D instance = Prefab.Instantiate<Node3D>();
            instance.Position = position;
            AddChild(instance);
            return instance;
        }
        return null;
    }

    private Vector3I GetRandomDirection() {
        Vector3I[] directions = AllowVerticalMovement
            ? new Vector3I[] {
                Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back, Vector3I.Up, Vector3I.Down
            }
            : new Vector3I[] {
                Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back
            };

        return directions[rng.RandiRange(0, directions.Length - 1)] * GridSize;
    }
}
