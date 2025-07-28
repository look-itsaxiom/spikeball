using Godot;
using Godot.Collections;
using Players;
using System;

public partial class StartUI : Control
{
    public Button StartButton;
    public HBoxContainer PlayerScoresContainer;
    public PackedScene ScoreLabelScene = ResourceLoader.Load<PackedScene>("res://Scenes/ScoreLabel.tscn");
    public Panel PreRoundTimerPanel;
    public Label PreRoundTimerLabel;
    public bool ListenForPreRoundTimer = false;
    public Label RoundTimerLabel;
    public Dictionary<int, Label> PlayerScoreLabels = new Dictionary<int, Label>();

    public override void _Ready()
    {
        StartButton = GetNode<Button>("StartButton");
        PlayerScoresContainer = GetNode<HBoxContainer>("PlayerScoresContainer");
        PreRoundTimerPanel = GetNode<Panel>("PreRoundTimerPanel");
        PreRoundTimerLabel = GetNode<Label>("PreRoundTimerPanel/PreRoundTimerLabel");
        RoundTimerLabel = GetNode<Label>("RoundTimerLabel");
        StartButton.Pressed += OnStartButtonPressed;
    }

    private void OnStartButtonPressed()
    {
        GameManager.Instance.StartGame();
        StartButton.Disabled = true;
        StartButton.Visible = false;
    }

    public void CreatePlayerScoreUI(Dictionary<int, Player> players)
    {
        var existingScoreLabels = PlayerScoresContainer.GetChildren();
        foreach (var label in existingScoreLabels)
        {
            if (label is Label scoreLabel)
            {
                PlayerScoresContainer.RemoveChild(scoreLabel);
                scoreLabel.QueueFree();
            }
        }

        PlayerScoreLabels.Clear();

        foreach (var player in players.Values)
        {
            var scoreLabel = ScoreLabelScene.Instantiate<Label>();
            scoreLabel.Text = "0";
            scoreLabel.LabelSettings = new LabelSettings
            {
                FontColor = PlayerData.PlayerColors[player.Color],
            };
            PlayerScoresContainer.AddChild(scoreLabel);
            PlayerScoreLabels[player.Id] = scoreLabel;
        }
    }

    public void UpdatePlayerScores(Dictionary<int, int> playerScores)
    {
        foreach (var score in playerScores)
        {
            if (PlayerScoreLabels.TryGetValue(score.Key, out var scoreLabel))
            {
                scoreLabel.Text = score.Value.ToString();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (ListenForPreRoundTimer)
        {
            PreRoundTimerLabel.Text = ((int)GameManager.Instance.PreRoundTimer.TimeLeft).ToString();
        }

        RoundTimerLabel.Text = ((int)GameManager.Instance.RoundTimer.TimeLeft).ToString();
    }
}
