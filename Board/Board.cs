using Godot;
using System;
using System.Collections.Generic;

public partial class Board : Node3D
{
    [Signal]
    public delegate void SquareClickedEventHandler(int x, int y);

    [Export]
    public PackedScene squareScene;
    [Export]
    public PackedScene pieceScene;

    // Empty child node that is the parent of all the pieces. This exists so we can easily destroy
    // all the pieces when we want to reset the board.
    [Export]
    public Node3D piecesNode;

    // Arrays containing references to the piece/square at a given coordinate. Basically a map
    // between coordinates and nodes.
    //
    // The squares array will be fully populated, but the pieces array will be sparse since there
    // are inherrently fewer pieces than squares.
    private Piece[,] pieces = new Piece[9, 9];
    private Square[,] squares = new Square[9, 9];

    private float pieceY = 0.464f;
    private float squareY = 0.374f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Create the tiles on the board
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                var square = (Square)squareScene.Instantiate();
                // The square needs to know its own coordinate to send signals to the controller
                square.CoordX = j;
                square.CoordY = i;
                square.Position = new Vector3(6 - 1.5f * j, squareY, -6 + 1.5f * i);
                square.Clicked += OnSquareClicked;
                squares[j, i] = square;
                AddChild(square);
            }
        }
    }

    // Sets up the board to the initial state of a shogi game
    public void ResetBoard()
    {
        // Destroy all the existing child pieces and recreate them
        foreach (Node n in piecesNode.GetChildren())
        {
            n.QueueFree();
        }

        SetupPieces();
    }

    // Highlights the given squares (and unhighlights any others)
    public void HighlightSquares(IEnumerable<(int, int)> coords)
    {
        UnhighlightAllSquares();

        foreach ((int x, int y) in coords)
        {
            squares[x, y].Highlight(true);
        }
    }

    // Sets all squares to be unhighlighted
    public void UnhighlightAllSquares()
    {
        foreach (Square s in squares)
        {
            s.Highlight(false);
        }
    }

    private void OnSquareClicked(int x, int y)
    {
        // Propagate up to the game controller
        EmitSignal(SignalName.SquareClicked, x, y);
    }

    private void SetupPieces()
    {
        // Place the pieces on the board
        SetupPiecesForSide(Player.Sente);
        SetupPiecesForSide(Player.Gote);
    }

    private void SetupPiecesForSide(Player player)
    {
        for (int x = 0; x < 9; x++)
        {
            PlacePiece(PieceType.Pawn, player, x, 2);
        }

        PlacePiece(PieceType.Rook, player, 7, 1);
        PlacePiece(PieceType.Bishop, player, 1, 1);

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

    private void PlacePiece(PieceType type, Player player, int x, int y)
    {
        if (player == Player.Gote)
        {
            x = 9 - x - 1;
            y = 9 - y - 1;
        }

        var piece = (Piece)pieceScene.Instantiate();
        piece.SetupPiece(type, player);

        if (player == Player.Sente)
        {
            piece.RotateY(Mathf.Pi);
        }
        piece.Position = new Vector3(6 - 1.5f * x, pieceY, -6 + 1.5f * y);
        pieces[x, y] = piece;
        piecesNode.AddChild(piece);
    }
}
