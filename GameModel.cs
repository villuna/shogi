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
