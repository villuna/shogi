using Godot;
using System;

public enum Player
{
    // The player who goes first
    Sente,
    // The player who goes second
    Gote,
}

// GameController - This class is the Controller in the Model/View/Controller framework.
//
// The Piece and Square nodes in the scene are the view, displaying to the user the current state
// of the game. They also collect user input and pass it up to this class using signals. The
// GameController processes this input, updates the internal model and then updates the visual
// nodes to represent the new state. As such all the game logic is processed here.
public partial class GameController : Node3D
{
    [Export]
    public Label turnIndicator;
    [Export]
    public Board board;

    // -- Board Model -- //
    // The structs defined below are data types used for the model of the game board.

    // Data type representing a piece owned by a player
    struct PieceData
    {
        public PieceType piece;
        public bool promoted;
        public Player player;

        public PieceData(PieceType piece, Player player, bool promoted)
        {
            this.piece = piece;
            this.player = player;
            this.promoted = promoted;
        }
    }

    private PieceData?[,] boardModel = new PieceData?[9, 9];
    private (int, int)? selected = null;

    // The player whose turn it is.
    private Player currentPlayer = Player.Sente;

    public override void _Ready()
    {
        SetupPieces();
    }

    private void SetupPieces()
    {
        SetupPiecesForPlayer(Player.Sente);
        SetupPiecesForPlayer(Player.Gote);
        board.ResetBoard();
    }

    private void SetupPiecesForPlayer(Player player)
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

    private void PlacePiece(PieceType piece, Player player, int x, int y)
    {
        if (player == Player.Gote)
        {
            x = 9 - x - 1;
            y = 9 - y - 1;
        }

        boardModel[x, y] = new PieceData(piece, player, false);
    }

    private void OnBoardSquareClicked(int x, int y)
    {
        GD.Print("Square (" + x + ", " + y + ") has been clicked");
        // If the player clicks on one of its own pieces, select that piece
        if (boardModel[x, y] != null && boardModel[x, y]?.player == currentPlayer)
        {
            selected = (x, y);
            board.HighlightSquares([(x, y)]);
        }
        else
        {
            selected = null;
            board.UnhighlightAllSquares();
        }
    }

    // Finishes the current player's turn and starts the next player's turn.
    private void NextTurn()
    {
        if (currentPlayer == Player.Sente)
        {
            currentPlayer = Player.Gote;
            turnIndicator.Text = "Gote's Turn";
        }
        else
        {
            currentPlayer = Player.Sente;
            turnIndicator.Text = "Sente's Turn";
        }
    }
}
