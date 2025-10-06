#nullable enable

using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public required Label turnIndicator;
    [Export]
    public required Board board;

    // -- Board Model -- //
    // The structs defined below are data types used for the model of the game board.

    // Data type representing a piece owned by a player
    public struct PieceData
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
        // Set up the actual nodes and models to represent the state of the board
        board.SetupBoard(boardModel);
    }

    private void SetupPiecesForPlayer(Player player)
    {
        for (int x = 0; x < 9; x++)
            PlacePiece(PieceType.Pawn, player, x, 2);

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
        if (boardModel[x, y] is PieceData piece && piece.player == currentPlayer)
        {
            // If this piece is already selected we should deselect it, else we should select it.
            if (selected == (x, y))
            {
                selected = null;
                board.UnhighlightAllSquares();
            }
            else
            {
                selected = (x, y);

                var movableSquares = MovableSquares(x, y);
                movableSquares.Add((x, y));
                board.HighlightSquares(movableSquares);
            }
        }
        else
        {
            if (selected is (int, int) s)
            {
                int sx = s.Item1, sy = s.Item2;
                var movableSquares = MovableSquares(sx, sy);

                if (movableSquares.Contains((x, y)))
                {
                    MovePiece(s, (x, y));
                    NextTurn();
                }
            }

            selected = null;
            board.UnhighlightAllSquares();
        }
    }

    private void MovePiece((int x, int y) from, (int x, int y) to)
    {
        Debug.Assert(boardModel[from.x, from.y] != null);
        Debug.Assert(boardModel[from.x, from.y]?.player == currentPlayer);
        Debug.Assert(CanMoveTo(to.x, to.y, currentPlayer));

        // TODO place captured pieces in holding so they can be placed again
        bool isCapturing = boardModel[to.x, to.y] != null;

        // Update the model
        boardModel[to.x, to.y] = boardModel[from.x, from.y];
        boardModel[from.x, from.y] = null;

        // Update the view
        if (isCapturing)
        {
            board.RemovePiece(to);
        }
        board.MovePiece(from, to);
    }

    // Given the coordinate of a piece on the board, returns which squares it can move to given
    // the current board setup. Throws an exception if there is no piece on the board.
    //
    // TODO: handle promoted pieces
    private List<(int, int)> MovableSquares(int x, int y)
    {
        if (boardModel[x, y] is PieceData piece)
        {
            var squares = new List<(int, int)>();

            if (piece.piece == PieceType.Pawn)
            {
                // Pawns can only move one square forward
                // unlike in chess, shogi pawns capture in the same way they move
                int step = piece.player == Player.Sente ? 1 : -1;

                if (CanMoveTo(x, y + step, piece.player))
                    squares.Add((x, y + step));
            }
            else if (piece.piece == PieceType.Bishop)
            {
                // The bishop can go as far as it wants in any of the 4 diagonal directions
                foreach ((int dx, int dy) in new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) })
                {
                    // For each of the 4 diagonal directions, try moving as far as possible
                    // in that direction.
                    var x2 = x + dx;
                    var y2 = y + dy;

                    while (CanMoveTo(x2, y2, piece.player))
                    {
                        squares.Add((x2, y2));

                        if (HasEnemyPiece(x2, y2, piece.player))
                            break;

                        x2 += dx;
                        y2 += dy;
                    }
                }
            }
            else if (piece.piece == PieceType.Rook)
            {
                // The rook can go as far as it wants in any of the 4 lateral directions
                foreach ((int dx, int dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    // For each lateral direction, try moving as far as possible in that
                    // direction
                    var x2 = x + dx;
                    var y2 = y + dy;

                    while (CanMoveTo(x2, y2, piece.player))
                    {
                        squares.Add((x2, y2));

                        if (HasEnemyPiece(x2, y2, piece.player))
                            break;

                        x2 += dx;
                        y2 += dy;
                    }
                }
            }
            else if (piece.piece == PieceType.Lance)
            {
                // The lance goes as far as it wants but forward only
                int step = piece.player == Player.Sente ? 1 : -1;
                var y2 = y + step;

                while (CanMoveTo(x, y2, piece.player))
                {
                    squares.Add((x, y2));

                    if (HasEnemyPiece(x, y2, piece.player))
                        break;

                    y2 += step;
                }
            }
            else if (piece.piece == PieceType.Knight)
            {
                // The knight goes forward 2 squares and then 1 to the left/right
                // can "jump" over other pieces
                int step = piece.player == Player.Sente ? 1 : -1;
                foreach ((int dx, int dy) in new[] { (1, 2 * step), (-1, 2 * step) })
                    if (CanMoveTo(x + dx, y + dy, piece.player))
                        squares.Add((x + dx, y + dy));
            }
            else if (piece.piece == PieceType.Silver)
            {
                // The silver general can move 1 square diagonally in any direction, or 1 square
                // forward.
                //
                // 000
                //  x
                // 0 0
                //
                // Like this, where x is the piece and 0 is a square the piece can move to.
                int step = piece.player == Player.Sente ? 1 : -1;
                foreach ((int dx, int dy) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1), (0, step) })
                    if (CanMoveTo(x + dx, y + dy, piece.player))
                        squares.Add((x + dx, y + dy));
            }
            else if (piece.piece == PieceType.Gold)
            {
                // The gold general can move 1 square laterally in any direction or diagonally
                // forward.
                //
                // 000
                // 0x0
                //  0
                //
                // Like this, where x is the piece and 0 is a square the piece can move to.
                int step = piece.player == Player.Sente ? 1 : -1;
                foreach ((int dx, int dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1), (-1, step), (1, step) })
                    if (CanMoveTo(x + dx, y + dy, piece.player))
                        squares.Add((x + dx, y + dy));
            }
            else if (piece.piece == PieceType.King)
            {
                // The king can move one square in any direction.
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        if (CanMoveTo(x + dx, y + dy, piece.player))
                            squares.Add((x + dx, y + dy));
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }

            return squares;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    private bool HasEnemyPiece(int x, int y, Player player)
    {
        return boardModel[x, y] is PieceData p && p.player != player;
    }

    private bool CanMoveTo(int x, int y, Player player)
    {
        return x >= 0 && y >= 0 && x < 9 && y < 9 &&
            !(boardModel[x, y] is PieceData p && p.player == player);
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
