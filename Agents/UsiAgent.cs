using Godot;
using System;

public partial class UsiAgent : Node
{
    [Export]
    public String processPath;

    private FileAccess processStdio;

    public override void _Ready()
    {
        Godot.Collections.Dictionary dict =
            OS.ExecuteWithPipe(processPath, [""], blocking: false);
        processStdio = (FileAccess)dict["stdio"];
        processStdio.StoreLine("usi");
        processStdio.StoreLine("go movetime 5000");
    }

    public override void _Process(double delta)
    {
        String nextLine = processStdio.GetLine();
        if (nextLine.Length != 0)
        {
            GD.Print("Got: \"" + nextLine + "\"");
            HandleUsiMessage(nextLine);
        }
    }

    private void HandleUsiMessage(String msg)
    {
        UsiMessage message = UsiMessage.Parse(msg);

        if (message is Info info)
        {
            GD.Print("Info: \"" + info.info + "\"");
        }
        else if (message is BestMove move)
        {
            if (move.best.type == Move.MoveType.Move)
            {
                GD.Print("Best Move: " + move.best.fromCoord + " to " + move.best.toCoord);
            }
            else
            {
                GD.Print("Best Move: drop " + move.best.dropType + " on " + move.best.toCoord);
            }
        }
    }
}
