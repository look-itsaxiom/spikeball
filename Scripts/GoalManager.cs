using Godot;
using Players;
using System;

public partial class GoalManager : Node2D
{
    public AnimatedSprite2D GoalNetSprite;
    public static Area2D GoalArea;

    public override void _Ready()
    {
        GoalNetSprite = GetNode<AnimatedSprite2D>("GoalNetSprite");
        GoalArea = GetNode<Area2D>("GoalNetSprite/GoalNet");
        GoalArea.BodyEntered += OnGoalNetEntered;
        GetParent<SpikeballManager>().BallPlayerChanged += OnBallPlayerChanged;
    }

    private void OnBallPlayerChanged()
    {
        Player player = GetParent<SpikeballManager>().GetBallPlayer();
        GoalNetSprite.Modulate = player.Color;
    }

    private void OnGoalNetEntered(Node2D body)
    {
        if (body is PlayerController playerController && playerController.PlayerMode == PlayerMode.Ball)
        {
            // only count a goal if the player enters the goal from the top
            if (playerController.GlobalPosition.Y < GlobalPosition.Y)
            {
                GoalNetSprite.Play("goal");
                GetParent<SpikeballManager>().EmitSignal("GoalScored");
            }
        }
    }
}
