using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class BaseGenerator : Node3D
{
    [Export] public PackedScene Room; 
    [Export] public PackedScene Wall; 
    [Export] public int Steps = 50; 
	[Export] public int Walks = 3;
    [Export] public int GridSize = 2; 
    [Export] public bool AllowVerticalMovement = false; 
    [Export] public Button DebugButton;
    
    private HashSet<Vector3I> visitedPositions = new();
    private RandomNumberGenerator rng = new();
    private Vector3I currentPosition = Vector3I.Zero;

    private HashSet<Vector3I> createdWalls = new();

    public override void _Ready()
    {
        GenerateBase();

        DebugButton.Pressed += GenerateBase;
    }

    public void GenerateBase() {
        visitedPositions = new();
        createdWalls = new();

        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        rng.Randomize();
		for (int i = 0; i < Walks; i++) {
			currentPosition = Vector3I.Zero;
			GenerateRandomWalk();
		}

		GenerateWalls();
    }

	private void GenerateWalls() {
		foreach (Vector3I room in visitedPositions) {
			var neighbours = GetNeighbours(room);

			foreach (var neighbour in neighbours) {
				var midpoint = (neighbour + room) / 2;
				if (!visitedPositions.Contains(neighbour) && !createdWalls.Contains(midpoint)) {
					var wall = SpawnPrefab(Wall, midpoint);	
					createdWalls.Add(midpoint);

                    // need to rotate
                    if (Math.Abs((neighbour - room).Z) > 0) {
                        // 90 degrees
                        wall.RotateY(Mathf.Pi / 2f);
                    }
				}
			}
		}
	}

	private Vector3I[] GetNeighbours(Vector3I center) {
		var neighbours = new Vector3I[] { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back };

		for (int i = 0; i < neighbours.Length; i++) {
            neighbours[i] *= 2;
			neighbours[i] += center;
		} 

		return neighbours;
	}

    private void GenerateRandomWalk()
    {
        for (int i = 0; i < Steps; i++)
        {
            if (!visitedPositions.Contains(currentPosition))
            {
                SpawnPrefab(Room, currentPosition);
                visitedPositions.Add(currentPosition);
            }

            currentPosition += GetRandomDirection();
        }
    }

    private Node3D SpawnPrefab(PackedScene Prefab, Vector3I position)
    {
        if (Prefab != null)
        {
            Node3D instance = Prefab.Instantiate<Node3D>();
            instance.Position = position;
            AddChild(instance);
            return instance;
        }
        return null;
    }

    private Vector3I GetRandomDirection()
    {
        Vector3I[] directions = AllowVerticalMovement 
            ? new Vector3I[] { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back, Vector3I.Up, Vector3I.Down } 
            : new Vector3I[] { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back };

        return directions[rng.RandiRange(0, directions.Length - 1)] * GridSize;
    }
}
