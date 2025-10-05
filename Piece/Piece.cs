using Godot;
using Godot.Collections;

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

public partial class Piece : Node3D
{
    [Export]
    public Array<Texture2D> FaceSprites;
    [Export]
    public Decal FaceDecal;

    public void SetupPiece(PieceType piece, Player player)
    {
        if (piece == PieceType.King && player == Player.Gote)
        {
            // The gote king (prince) has a different sprite from the sente king, which is stored
            // at the end of the array
            FaceDecal.TextureAlbedo = FaceSprites[(int)piece + 1];
        }
        else
        {
            FaceDecal.TextureAlbedo = FaceSprites[(int)piece];
        }
    }
}
