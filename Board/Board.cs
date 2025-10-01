using Godot;
using System;

public partial class Board : Node3D
{
	private PackedScene squareScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        squareScene = GD.Load<PackedScene>("res://Board/square.tscn");
        // Create the tile decals on the board
        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                var tile = (Node3D)squareScene.Instantiate();
                tile.Position = new Vector3(-6 + 1.5f * (float)i, 0.374f, -6 + 1.5f * (float)j);
                AddChild(tile);
            }

        }

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
