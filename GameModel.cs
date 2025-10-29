// GameModel.cs - Types and functions for representing the state of the game in code.
// These classes are not Godot nodes but are just used for modelling the game.

using System;

public enum PieceType
{
    Pawn = 0,
    Bishop,
    Rook,
    Lance,
    Knight,
    Silver,
    Gold,
    King
}

public enum Player
{
    // The player who goes first
    Sente,
    // The player who goes second
    Gote,
}

// Datatype representing a piece in the game
public class PieceData
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

    public bool CanPromote(int y)
    {
        if (promoted)
            return false;

        // Check if we are on one of the right ranks
        if (!(player == Player.Sente && y >= 6 || player == Player.Gote && y <= 2))
        {
            return false;
        }

        if (piece == PieceType.King || piece == PieceType.Gold)
        {
            return false;
        }

        return true;
    }
}

public struct Move
{
    public enum MoveType
    {
        Move,
        Drop,
    }
    public MoveType type;
    public (int x, int y)? fromCoord;
    public PieceType? dropType;
    public (int x, int y) toCoord;

    // Parse a move from string in algebraic notation
    public Move(string str)
    {
        string pieceChars = "RBGSNLP";
        string cols = "123456789";
        string rows = "abcdefghi";

        if (pieceChars.Contains(str[0]))
        {
            this.type = MoveType.Drop;
            if (str.Length != 4 || str[1] != '*'
                || !cols.Contains(str[2]) || !rows.Contains(str[3]))
            {
                throw new ArgumentException("Invalid move notation: " + str);
            }
            this.toCoord = (str[2] - '1', str[3] - 'a');
        }
        else if (cols.Contains(str[0]))
        {
            this.type = MoveType.Move;

            if (!(str.Length == 4 || str.Length == 5) || !rows.Contains(str[1])
                || !cols.Contains(str[2]) || !rows.Contains(str[3]))
            {
                throw new ArgumentException("Invalid move notation: " + str);
            }
            this.fromCoord = (str[0] - '1', str[1] - 'a');
            this.toCoord = (str[2] - '1', str[3] - 'a');
        }
    }
}
