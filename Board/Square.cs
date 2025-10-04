using Godot;
using System;

public partial class Square : Node3D
{
    private bool mouseEntered = false;
    public int CoordX { get; set; }
    public int CoordY { get; set; }

    [Export]
    public Sprite3D selectionSprite;

    [Signal]
    public delegate void ClickedEventHandler(int x, int y);

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("click") && mouseEntered)
        {
            EmitSignal(SignalName.Clicked, CoordX, CoordY);
        }
    }

    private void OnMouseDetectorMouseEntered()
    {
        mouseEntered = true;
    }

    private void OnMouseDetectorMouseExited()
    {
        mouseEntered = false;
    }
}
