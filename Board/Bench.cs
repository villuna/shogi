#nullable enable

using Godot;

public partial class Bench : Node3D
{
    // We have to pass the piece type and player as ints because the custom types aren't Godot
    // variants.
    [Signal]
    public delegate void SquareClickedEventHandler(int piece, int player);

    [Export]
    public required PackedScene squareScene;
    [Export]
    public required PackedScene pieceScene;

    // The player whose bench this is. Used for signalling to the GameController.
    [Export]
    public Player player;

    private Piece[] pieces = new Piece[7];
    private Square[] squares = new Square[8];

    private float squareY = 0.374f;
    private float pieceY = 0.464f;

    public void SetupBench(Player player)
    {
        // First we should clean up existing Piece nodes
        foreach (Piece p in pieces)
            if (p != null)
                p.QueueFree();

        // Create and initialise the piece models that will display how many of each piece the
        // player has. There are 7 capturable pieces, but the bench is a grid of 4x2 squares so
        // one of the squares will be intentionally left empty.
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                int index = y * 2 + x;
                PieceType type = (PieceType)index;
                // Kings are not capturable so we don't need to set up a piece for it
                if (type == PieceType.King)
                    continue;

                Piece piece = pieceScene.Instantiate<Piece>();
                piece.Position = new Vector3(0.75f - 1.5f * x, pieceY, 2.25f - 1.5f * y);
                if (player == Player.Gote)
                {
                    piece.RotateCountLabel();
                }
                pieces[index] = piece;
                piece.SetupPiece(type, player);
                // No pieces captured at the beginning of the game
                piece.DisplayCount(0);
                AddChild(piece);
            }
        }
    }

    public override void _Ready()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                int index = y * 2 + x;
                Square square = squareScene.Instantiate<Square>();
                square.CoordX = x;
                square.CoordY = y;
                square.Clicked += OnSquareClicked;
                square.Position = new Vector3(0.75f - 1.5f * x, squareY, 2.25f - 1.5f * y);
                AddChild(square);
                squares[index] = square;
            }
        }
    }

    // Sets the piece count for a certain piece. This shows the user how many of that piece is on a
    // player's bench
    public void UpdateCountForPiece(PieceType piece, int count)
    {
        pieces[(int)piece].DisplayCount(count);
    }

    // Highlights the square containing pieces of the given type. Indicates that this piece is
    // selected.
    public void HighlightPiece(PieceType piece)
    {
        UnhighlightAllSquares();
        squares[(int)piece].Highlight(true);
    }

    public void UnhighlightAllSquares()
    {
        foreach (Square s in squares)
            s.Highlight(false);
    }

    private void OnSquareClicked(int x, int y)
    {
        EmitSignal(SignalName.SquareClicked, 2 * y + x, (int)player);
    }
}
