using Godot;
using System;

public partial class MainMenu : Node3D
{
    [Export]
    public Piece pieceModel;
    [Export]
    public float pieceRotateSpeed;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pieceModel.SetupPiece(PieceType.Rook, Player.Sente);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        pieceModel.RotateY(pieceRotateSpeed * (float)delta);
    }

    private void OnPlayPressed()
    {
        // I would rather this be an export var but we would get a circular scene dependency :/
        GetTree().ChangeSceneToFile("res://Game/Main.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
