using Godot;
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
    public void SetupBoard(GameController.PieceData?[,] board)
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                PlacePiece(board[x, y], x, y);
            }
        }
    }

    private void PlacePiece(GameController.PieceData? pieceData, int x, int y)
    {
        if (pieceData is GameController.PieceData data)
        {
            var piece = (Piece)pieceScene.Instantiate();
            piece.SetupPiece(data.piece, data.player);

            if (data.player == Player.Sente)
            {
                piece.RotateY(Mathf.Pi);
            }
            piece.Position = new Vector3(6 - 1.5f * x, pieceY, -6 + 1.5f * y);
            pieces[x, y] = piece;
            piecesNode.AddChild(piece);
        }
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
}
