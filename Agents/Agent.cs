using Godot;
using System;

public abstract partial class Agent : Node
{
    [Signal]
    public delegate void MoveMadeEventHandler(string move);

    public abstract void StartTurn(string sfenPosition);
}
