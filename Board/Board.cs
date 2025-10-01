using Godot;
using System;
using System.IO.Pipes;

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
        SetupPiecesForSide(Player.Sente);
        SetupPiecesForSide(Player.Gote);
    }

    private void SetupPiecesForSide(Player player) {
        for (int x = 0; x < 9; x++) {
            PlacePiece(PieceType.Pawn, player, x, 2);
        }

        PlacePiece(PieceType.Rook, player, 1, 1);
        PlacePiece(PieceType.Bishop, player, 7, 1);

        PlacePiece(PieceType.Lance, player, 0, 0);
        PlacePiece(PieceType.Lance, player, 8, 0);

        PlacePiece(PieceType.Knight, player, 1, 0);
        PlacePiece(PieceType.Knight, player, 7, 0);

        PlacePiece(PieceType.Silver, player, 2, 0);
        PlacePiece(PieceType.Silver, player, 6, 0);

        PlacePiece(PieceType.Gold, player, 3, 0);
        PlacePiece(PieceType.Gold, player, 5, 0);

        PlacePiece(PieceType.King, player, 4, 0);
    }

    private void PlacePiece(PieceType type, Player player, int x, int y) {
        if (player == Player.Sente) {
            x = 9 - x - 1;
            y = 9 - y - 1;
        }

        var piece = (Piece)pieceScene.Instantiate();
        piece.SetupPiece(type, player);

        if (player == Player.Gote) {
            piece.RotateY(Mathf.Pi);
        }
        piece.Position = new Vector3(-6 + 1.5f * x, pieceY, -6 + 1.5f * y);
        AddChild(piece);
    }
}
