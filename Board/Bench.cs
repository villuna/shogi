using Godot;
using System;

public partial class Bench : Node3D
{
    [Export]
    public PackedScene squareScene;
    private float squareY = 0.374f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Square square = squareScene.Instantiate<Square>();
                square.CoordX = x;
                square.CoordY = y;
                square.Clicked += OnSquareClicked;
                square.Position = new Vector3(0.75f - 1.5f * x, squareY, 1.5f - 1.5f * y);
                AddChild(square);
            }
        }
    }

    private void OnSquareClicked(int x, int y)
    {
        GD.Print("Bench square clicked: (" + x + ", " + y + ")");
    }
}
