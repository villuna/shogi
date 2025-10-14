#nullable enable

using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Board : Node3D
{
    // Signal that gets sent to the board every time a square is clicked
    [Signal]
    public delegate void SquareClickedEventHandler(int x, int y);

    [Export]
    public required PackedScene squareScene;
    [Export]
    public required PackedScene pieceScene;

    // Empty child node that is the parent of all the pieces. This exists so we can easily destroy
    // all the pieces when we want to reset the board.
    [Export]
    public required Node3D piecesNode;

    // Arrays containing references to the piece/square at a given coordinate. Basically a map
    // between coordinates and nodes.
    //
    // The squares array will be fully populated, but the pieces array will be sparse since there
    // are inherrently fewer pieces than squares.
    private Piece?[,] pieces = new Piece[9, 9];
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
                square.Position = PositionForCoords(j, i);
                square.Clicked += OnSquareClicked;
                squares[j, i] = square;
                AddChild(square);
            }
        }
    }

    // Calculates the position a piece should have given its boardspace coordinate.
    private Vector3 PositionForCoords(int x, int y)
    {
        return new Vector3(6 - 1.5f * x, squareY, -6 + 1.5f * y);
    }

    // Sets up the board to the initial state of a shogi game.
    public void SetupBoard(PieceData?[,] board)
    {
        // Destroy any existing pieces
        foreach (Node n in piecesNode.GetChildren())
            n.QueueFree();

        // Set up the board as described by the given model
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                if (board[x, y] is PieceData data)
                    PlacePiece(data, x, y);
    }

    // Moves a piece from one space on the board to another.
    // assumes the space it's moving to is empty and does not contain another piece.
    public void MovePiece((int x, int y) from, (int x, int y) to)
    {
        Debug.Assert(pieces[from.x, from.y] != null);
        Debug.Assert(pieces[to.x, to.y] == null);

        // Update the position of the piece
        if (pieces[from.x, from.y] is Piece p)
        {
            // This condition is always true bc of the assert but the C# compiler will give a
            // warning if I don't insert this if statement
            p.Position = PositionForCoords(to.x, to.y);
        }

        // Update our references accordingly
        pieces[to.x, to.y] = pieces[from.x, from.y];
        pieces[from.x, from.y] = null;
    }

    // Captures a piece from the board and puts it in the capturing player's bench
    public void CapturePiece((int x, int y) coord, Player capturingPlayer)
    {
        if (pieces[coord.x, coord.y] is Piece p)
        {
            p.QueueFree();
            pieces[coord.x, coord.y] = null;
        }
        else
        {
            Debug.Assert(false);
        }
    }

    private void PlacePiece(PieceData data, int x, int y)
    {
        var piece = (Piece)pieceScene.Instantiate();
        piece.SetupPiece(data.piece, data.player);

        if (data.player == Player.Sente)
            piece.RotateY(Mathf.Pi);

        piece.Position = PositionForCoords(x, y);
        pieces[x, y] = piece;
        piecesNode.AddChild(piece);
    }

    // Highlights the given squares (and unhighlights any others)
    public void HighlightSquares(IEnumerable<(int, int)> coords)
    {
        UnhighlightAllSquares();

        foreach ((int x, int y) in coords)
            squares[x, y].Highlight(true);
    }

    // Sets all squares to be unhighlighted
    public void UnhighlightAllSquares()
    {
        foreach (Square s in squares)
            s.Highlight(false);
    }

    private void OnSquareClicked(int x, int y)
    {
        // Propagate up to the game controller
        EmitSignal(SignalName.SquareClicked, x, y);
    }
}
