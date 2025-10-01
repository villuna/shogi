using Godot;
using System;

public enum Player {
	// The player who goes first
	Sente,
	// The player who goes second
	Gote,
}

public partial class GameController : Node3D
{
    [Export]
    public Label turnIndicator;

    private Player turn = Player.Sente;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    // Finishes the current player's turn and starts the next player's turn.
    private void NextTurn()
    {
        if (turn == Player.Sente)
        {
            turn = Player.Gote;
            turnIndicator.Text = "Gote's Turn";
        }
        else
        {
            turn = Player.Sente;
            turnIndicator.Text = "Sente's Turn";
        }
    }
}
