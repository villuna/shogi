using Godot;
using System;
using System.Collections.Generic;

public partial class UsiAgent : Agent
{
    [Export]
    public String processPath;
    [Export]
    public int moveTimeMillis;

    private FileAccess processStdio;
    private FileAccess processStderr;
    private Queue<String> commands = new Queue<String>();

    public override void _Ready()
    {
        Godot.Collections.Dictionary dict =
            OS.ExecuteWithPipe(processPath, [""], blocking: false);
        processStdio = (FileAccess)dict["stdio"];
        processStderr = (FileAccess)dict["stderr"];
        commands.Enqueue("usi");
    }

    public override void _Process(double delta)
    {
        // Write any pending commands
        string nextCommand;
        if (commands.TryDequeue(out nextCommand))
        {
            GD.Print("Sending command \"" + nextCommand + "\"");
            if (!processStdio.StoreLine(nextCommand))
            {
                throw new ApplicationException("Line not sent to process");
            }
        }

        // Read any pending messages
        String nextLine = processStdio.GetLine();
        if (nextLine.Length != 0)
        {
            GD.Print("Got: \"" + nextLine + "\"");
            HandleUsiMessage(nextLine);
        }

        string err = processStderr.GetAsText();
        if (err.Length != 0)
        {
            GD.Print("stderr: " + err);
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
            GD.Print("Best Move: " + move.bestMove);
            EmitSignal(SignalName.MoveMade, move.bestMove);
        }
    }

    public override void StartTurn(string sfenPosition)
    {
        GD.Print("Starting engine from position " + sfenPosition);
        commands.Enqueue("position sfen " + sfenPosition);
        commands.Enqueue("go movetime " + moveTimeMillis);
    }
}
