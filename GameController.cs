#nullable enable

using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public required Board boardNode;
    [Export]
    public required Bench[] benchNodes = new Bench[2];

    // -- Board Model -- //
    // The structs defined below are data types used for the model of the game board.

    private PieceData?[,] board = new PieceData?[9, 9];

    // Each player has a "bench" of pieces they've captured. For each player we store how many
    // of a certain piece they stored in their bench.
    //
    // This is a multidimensional array where the row index is the player and the column index is
    // the piece type. So, if Gote has 2 captured rooks in their storage then
    // `benches[Player.Gote, PieceType.Rook] == 2`.
    private int[,] benches = new int[2, 7];

    // The coordinate of the piece that is currently selected (or null if no board square is
    // selected).
    private (int, int)? selected = null;
    // The currently selected piece on the current player's bench (or null if none is selected).
    private PieceType? selectedBenchPiece = null;

    // The player whose turn it is.
    private Player currentPlayer = Player.Sente;

    public override void _Ready()
    {
        SetupPieces();
    }

    private void SetupPieces()
    {
        // Set up the actual nodes that represent the state of the board visually
        for (int i = 0; i < 2; i++)
        {
            Player p = (Player)i;
            SetupPiecesForPlayer(p);
            benchNodes[i].SetupBench(p);
        }
        boardNode.SetupBoard(board);
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

        board[x, y] = new PieceData(piece, player, false);
    }

    private void DeselectBenchPiece()
    {
        selectedBenchPiece = null;
        benchNodes[(int)currentPlayer].UnhighlightAllSquares();
    }

    private void DeselectBoardPiece()
    {
        selected = null;
        boardNode.UnhighlightAllSquares();
    }

    private void OnBoardSquareClicked(int x, int y)
    {
        if (board[x, y] is PieceData piece && piece.player == currentPlayer)
        {
            // If this piece is already selected we should deselect it, else we should select it.
            if (selected == (x, y))
            {
                DeselectBoardPiece();
            }
            else
            {
                selected = (x, y);

                var movableSquares = MovableSquares(x, y, true);
                movableSquares.Add((x, y));
                boardNode.HighlightSquares(movableSquares);
            }
        }
        else
        {
            GD.Print("Else branch");
            if (selected is (int, int) s)
            {
                GD.Print("Piece is selected");
                // Attempt to move the selected piece to the square the player clicked on
                int sx = s.Item1, sy = s.Item2;
                var movableSquares = MovableSquares(sx, sy, true);

                if (movableSquares.Contains((x, y)))
                {
                    MovePiece(s, (x, y));
                    NextTurn();
                }
            }
            else if (selectedBenchPiece is PieceType dropPiece && board[x, y] == null)
            {
                GD.Print("Bench piece is indeed selected");
                // Attempt to place the selected piece on the square the player clicked on
                if (benches[(int)currentPlayer, (int)dropPiece] > 0)
                {
                    GD.Print("We have enough pieces");
                    var droppableSquares = DroppableSquares(dropPiece);

                    if (droppableSquares.Contains((x, y)))
                    {
                        GD.Print("We can drop the piece");
                        DropPiece(dropPiece, (x, y));
                        NextTurn();
                    }
                }
            }

            DeselectBoardPiece();
        }

        // If we click on a board square and a piece on the bench was already selected it should be
        // deselected no matter what. This is after the big if statement because we need to know
        // what bench piece *was* selected in case we want to drop it onto the board.
        DeselectBenchPiece();
    }

    private void DropPiece(PieceType piece, (int x, int y) coord)
    {
        Debug.Assert(benches[(int)currentPlayer, (int)piece] > 0);
        Debug.Assert(board[coord.x, coord.y] == null);
        var data = new PieceData(piece, currentPlayer, false);
        board[coord.x, coord.y] = data;
        boardNode.PlacePiece(data, coord.x, coord.y);
        benches[(int)currentPlayer, (int)piece]--;
        benchNodes[(int)currentPlayer].UpdateCountForPiece(piece, benches[(int)currentPlayer, (int)piece]);
    }

    private void OnBenchSquareClicked(int pieceId, int playerId)
    {
        Player player = (Player)playerId;
        PieceType piece = (PieceType)pieceId;

        // Ignore any clicks that are on the enemy's bench
        if (player != currentPlayer)
        {
            return;
        }

        // If a piece is currently selected on the board it should be deselected
        DeselectBoardPiece();

        if (selectedBenchPiece is PieceType sp && sp == piece)
        {
            // Clicking on the selected piece should deselect it
            DeselectBenchPiece();
        }
        else
        {
            // Only select pieces that are actually stored in the bench
            if (pieceId != 7 && benches[playerId, pieceId] > 0)
            {
                selectedBenchPiece = piece;
                benchNodes[playerId].HighlightPiece(piece);
                List<(int, int)> droppableSquares = DroppableSquares(piece);
                boardNode.HighlightSquares(droppableSquares);
            }
            else
            {
                // If they clicked on an empty square on the bench we should deselect the bench
                // piece too.
                DeselectBenchPiece();
            }
        }
    }

    private void MovePiece((int x, int y) from, (int x, int y) to)
    {
        Debug.Assert(board[from.x, from.y] != null);
        Debug.Assert(board[from.x, from.y]?.player == currentPlayer);
        Debug.Assert(CanMoveTo(to.x, to.y, currentPlayer));

        // Hold onto the captured piece to update the benches
        var capturedPiece = board[to.x, to.y];

        // Update the board model
        board[to.x, to.y] = board[from.x, from.y];
        board[from.x, from.y] = null;

        // Handle captured pieces
        if (capturedPiece is PieceData p)
        {
            benches[(int)currentPlayer, (int)p.piece] += 1;

            boardNode.CapturePiece(to, currentPlayer);
            benchNodes[(int)currentPlayer].UpdateCountForPiece(p.piece, benches[(int)currentPlayer, (int)p.piece]);
        }

        // Update the board view
        boardNode.MovePiece(from, to);
    }

    // Given the coordinate of a piece on the board, returns which squares it can move to given
    // the current board setup. Throws an exception if there is no piece on the board.
    //
    // If `disallowCheck` is enabled, this function won't return moves that would put the current
    // player's king into check. Otherwise it skips this check entirely.
    //
    // TODO: handle promoted pieces
    private List<(int, int)> MovableSquares(int x, int y, bool disallowCheck)
    {
        if (board[x, y] is PieceData piece)
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

            if (disallowCheck)
            {
                // Now we have to go through each possible move and filter out those which would put
                // the king in check. This has the added benefit of filtering out all moves that don't
                // move the king out of check if it is in it currently.
                for (int i = squares.Count - 1; i >= 0; i--)
                {
                    // Since we are removing squares we have to iterate backwards over the list so as
                    // to not invalidate indexes.

                    // Temporarily move the piece to that square, then check if the king is in check.
                    // Then move the pieces back to where they were. Kind of a weird way of doing it
                    // but it works well and avoids cloning the model.
                    (int x2, int y2) = squares[i];

                    var tmp = board[x2, y2];
                    board[x2, y2] = board[x, y];
                    board[x, y] = null;

                    bool inCheck = PlayersKingInCheck(currentPlayer);

                    if (inCheck)
                    {
                        squares.RemoveAt(i);
                    }

                    board[x, y] = board[x2, y2];
                    board[x2, y2] = tmp;
                }
            }

            return squares;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    // Returns a list of all the squares where the current player can drop a certain piece.
    // The player can drop a piece on any square in the board, with one exception - a player is
    // not allowed to have two or more pawns on the same file.
    //
    // Check rules apply so this function will not return moves that would leave the current
    // player's king in check.
    private List<(int, int)> DroppableSquares(PieceType piece)
    {
        // Can't drop a king
        if (piece == PieceType.King)
            throw new ArgumentOutOfRangeException();

        var squares = new List<(int, int)>();

        // Build up the list of squares column by column, so if we run into overlapping pawns we
        // can filter out the entire column.
        for (int x = 0; x < 9; x++)
        {
            var squaresInColumn = new List<(int, int)>();

            for (int y = 0; y < 9; y++)
            {
                PieceData? pieceData = board[x, y];

                if (pieceData is PieceData pd)
                {
                    // If the piece we are placing is a pawn and we find a pawn in this column from
                    // the current player, we need to discard this column and move on.
                    if (piece == PieceType.Pawn && pd.piece == PieceType.Pawn
                        && currentPlayer == pd.player)
                    {
                        squaresInColumn.Clear();
                        break;
                    }
                }
                else
                {
                    squaresInColumn.Add((x, y));
                }
            }

            squares.AddRange(squaresInColumn);
        }

        // Filter out squares that would leave the king in check
        for (int i = squares.Count - 1; i >= 0; i--)
        {
            // Place the piece down, check if the king is in check, and then reset the board
            // back to how it was.
            (int x, int y) square = squares[i];
            Debug.Assert(board[square.x, square.y] == null);

            board[square.x, square.y] = new PieceData(piece, currentPlayer, false);

            if (PlayersKingInCheck(currentPlayer))
                squares.RemoveAt(i);

            board[square.x, square.y] = null;
        }

        return squares;
    }

    private bool PlayersKingInCheck(Player player)
    {
        // The way this is implemented is not very performant.
        // In the future I might like to pre-calculate all the squares reachable by each side's
        // pieces at the start of each turn so we don't have to keep doing it like this.
        // But this is good enough for now.
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                // Loop over every enemy piece
                if (board[x, y] is PieceData p && p.player != player)
                {
                    var squares = MovableSquares(x, y, false);
                    foreach ((int x2, int y2) in squares)
                    {
                        if (board[x2, y2] is PieceData p2 && p2.player == player
                            && p2.piece == PieceType.King)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool HasEnemyPiece(int x, int y, Player player)
    {
        return board[x, y] is PieceData p && p.player != player;
    }

    private bool CanMoveTo(int x, int y, Player player)
    {
        return x >= 0 && y >= 0 && x < 9 && y < 9 &&
            !(board[x, y] is PieceData p && p.player == player);
    }

    // Finishes the current player's turn and starts the next player's turn.
    private void NextTurn()
    {
        DeselectBenchPiece();
        DeselectBoardPiece();

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
