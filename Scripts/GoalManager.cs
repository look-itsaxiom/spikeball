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
        GameManager.Instance.BallPlayerChanged += OnBallPlayerChanged;
    }

    private void OnBallPlayerChanged()
    {
        Player player = GameManager.Instance.GetBallPlayer();
        GoalNetSprite.Modulate = PlayerData.PlayerColors[player.Color];
    }

    private void OnGoalNetEntered(Node2D body)
    {
        if (body is PlayerController playerController && playerController.PlayerMode == PlayerMode.Ball)
        {
            GoalNetSprite.Play("goal");
            GameManager.Instance.EmitSignal("GoalScored");
        }
    }
}
