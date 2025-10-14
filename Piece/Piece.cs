#nullable enable

using Godot;
using Godot.Collections;
using System;

public partial class Piece : Node3D
{
    [Export]
    public required Array<Texture2D> FaceTextures;
    [Export]
    public required Array<Texture2D> BackTextures;
    [Export]
    public required Sprite3D FaceSprite;
    [Export]
    public required Sprite3D BackSprite;
    [Export]
    public required Label3D CountLabel;
    [Export]
    public required Node3D CountLabelPivot;

    public void SetupPiece(PieceType piece, Player player)
    {
        if (piece == PieceType.King && player == Player.Gote)
        {
            // The gote king (prince) has a different sprite from the sente king, which is stored
            // at the end of the array
            FaceSprite.Texture = FaceTextures[(int)piece + 1];
            // Also the king has no back sprite
        }
        else
        {
            FaceSprite.Texture = FaceTextures[(int)piece];
            BackSprite.Texture = BackTextures[(int)piece];
        }
    }

    // Displays a number next to the piece. Used to indicate how many of a certain piece a player
    // has on their bench. If the count is 0 neither the piece nor the number will be visible - if
    // the count is 1, the piece will be visible as normal.
    public void DisplayCount(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("Not possible to have less than 0 of a type of piece on the player's bench");
        }
        if (count == 0)
        {
            Visible = false;
        }
        else if (count == 1)
        {
            Visible = true;
            CountLabel.Visible = false;
        }
        else
        {
            Visible = true;
            CountLabel.Visible = true;
            CountLabel.Text = count.ToString();
        }
    }

    // Rotates the piece count label 180 degrees. This is so that when the piece is facing towards
    // the camera, we can see the count right-side up.
    public void RotateCountLabel()
    {
        CountLabelPivot.RotateY(Mathf.Pi);
    }

    // Flips the piece over so it shows its promoted (back) face
    public void Promote()
    {
        RotateZ(Mathf.Pi);
    }
}
