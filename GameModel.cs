// GameModel.cs - Types and functions for representing the state of the game in code.
// These classes are not Godot nodes but are just used for modelling the game.

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
