using Godot;
using System;

public partial class Board : Node3D
{
	private PackedScene squareScene;
    private PackedScene pieceScene;
    private float pieceY = 0.48f;
    private float squareY = 0.374f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        squareScene = GD.Load<PackedScene>("res://Board/square.tscn");
        pieceScene = GD.Load<PackedScene>("res://Piece/piece.tscn");
        // Create the tile decals on the board
        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                var tile = (Node3D)squareScene.Instantiate();
                tile.Position = new Vector3(-6 + 1.5f * j, squareY, -6 + 1.5f * i);
                AddChild(tile);
            }
        }
        SetupPieces();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void SetupPieces() {
        // Place the pieces on the board
        SetupPiecesForSide(false);
        SetupPiecesForSide(true);
    }

    private void SetupPiecesForSide(bool sente) {
        // TODO actually set up different pieces
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 9; j++) {
                int row;

                if (sente) {
                    row = i;
                } else {
                    row = 9 - i - 1;
                }

                var piece = (Node3D)pieceScene.Instantiate();

                if (sente) {
                    piece.RotateY(Mathf.Pi);
                }

                piece.Position = new Vector3(-6 + 1.5f * j, pieceY, -6 + 1.5f * row);

                
                AddChild(piece);
            }
        }
    }
}
