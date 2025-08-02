using Godot;
using Godot.Collections;
using Players;
using System;
using System.Linq;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        GD.Print("GameManager initialized.");
        GD.Print("GameManager ready.");
    }
}
