#nullable enable

using Godot;
using Godot.Collections;

public partial class Piece : Node3D
{
    [Export]
    public required Array<Texture2D> FaceSprites;
    [Export]
    public required Decal FaceDecal;

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
