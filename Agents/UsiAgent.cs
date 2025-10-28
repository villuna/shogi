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
    }

    public override void _Process(double delta)
    {
        String nextLine = processStdio.GetLine();
        if (nextLine.Length != 0)
        {
            GD.Print("nano: \"" + nextLine + "\"");
        }
    }
}
