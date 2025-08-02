using Godot;
using Godot.Collections;
using Players;
using System;
using System.Linq;

public partial class SpikeballManager : Node2D
{
    public enum RoundResult
    {
        BallScored,
        BallStopped,
        Debug
    }

    public Dictionary<int, Player> Players { get; private set; } = new Dictionary<int, Player>();
    public Dictionary<int, PlayerController> PlayerControllers { get; private set; } = new Dictionary<int, PlayerController>();
    public Dictionary<int, int> PlayerScores { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, bool> PlayerHasBeenBallRecently { get; private set; } = new Dictionary<int, bool>();
    public const double ROUND_DURATION = 15.0;
    public const double PRE_ROUND_DURATION = 3.0;
    public const double POST_ROUND_DURATION = 2.0;

    public PackedScene BallScene;
    public PackedScene SpikeBallScene;

    public Timer RoundTimer;
    public Timer PreRoundTimer;
    public Timer PostRoundTimer;

    public StartUI StartUI { get; private set; }

    private bool roundEnded = false;

    [Signal]
    public delegate void BallPlayerChangedEventHandler();

    [Signal]
    public delegate void GoalScoredEventHandler();

    [Signal]
    public delegate void GoalStoppedEventHandler();

    public override void _Ready()
    {
        StartUI = GetNode<StartUI>("StartUI");
        GD.Print("SpikeballManager initialized.");
        BallScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player_Ball.tscn");
        SpikeBallScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player_Spikeball.tscn");
        this.GoalScored += () => EndRound(RoundResult.BallScored);
        this.GoalStopped += () => EndRound(RoundResult.BallStopped);
        RoundTimer = new Timer();
        RoundTimer.WaitTime = ROUND_DURATION;
        RoundTimer.OneShot = true;
        RoundTimer.Timeout += RoundEndTimeout;
        AddChild(RoundTimer);
        PreRoundTimer = new Timer();
        PreRoundTimer.WaitTime = PRE_ROUND_DURATION;
        PreRoundTimer.OneShot = true;
        PreRoundTimer.Timeout += StartRound;
        AddChild(PreRoundTimer);
        PostRoundTimer = new Timer();
        PostRoundTimer.WaitTime = POST_ROUND_DURATION;
        PostRoundTimer.OneShot = true;
        PostRoundTimer.Timeout += StartPostRound;
        AddChild(PostRoundTimer);
        GD.Print("SpikeballManager ready.");
        StartGame();
    }

    public void StartGame()
    {
        GD.Print("Starting game...");
        Players.Clear();
        PlayerControllers.Clear();
        PlayerScores.Clear();
        PlayerHasBeenBallRecently.Clear();

        var registeredPlayers = PlayerRegistrar.Instance.GetRegisteredPlayers();

        foreach (var playerRegistration in registeredPlayers)
        {
            Player player = new Player
            {
                Id = playerRegistration.Id,
                Mode = PlayerMode.Setup,
                Color = playerRegistration.Color
            };
            Players.Add(playerRegistration.Id, player);
            PlayerScores.Add(playerRegistration.Id, 0);
            PlayerHasBeenBallRecently.Add(playerRegistration.Id, false);
        }

        StartUI.CreatePlayerScoreUI(Players);
        StartPreRound();
    }

    public void StartPreRound()
    {
        GD.Print("Starting pre-round...");
        StartUI.PreRoundTimerPanel.Visible = true;
        StartUI.PreRoundTimerLabel.Visible = true;
        StartUI.PreRoundTimerLabel.Text = "Get Ready!";
        StartUI.ListenForPreRoundTimer = true;
        PreRoundTimer.Start();
    }

    private void StartRound()
    {
        GD.Print("Starting round...");
        roundEnded = false;
        GD.Print("Picking new ball player...");
        // just in case
        PlayerControllers.Clear();
        ClearPlayerControllers();
        var roundBallPlayer = PickBallPlayer();

        GD.Print($"New ball player: {roundBallPlayer.Id} - {roundBallPlayer.Mode}");
        GD.Print("Setting up ball player controller...");
        Node2D parentNode = GetTree().Root.GetNode<Node2D>("Core_Spikeball");

        PlayerController ballPlayer = BallScene.Instantiate<PlayerController>();
        PlayerControllers.Add(roundBallPlayer.Id, ballPlayer);
        Marker2D ballSpawn = parentNode.GetNode<Marker2D>("SpawnPoints/Ball");

        ballPlayer.InitPlayerState(roundBallPlayer);
        parentNode.AddChild(ballPlayer);
        ballPlayer.GlobalPosition = ballSpawn.GlobalPosition;
        GD.Print("Ball player controller setup complete.");
        GD.Print("Setting up spike ball players...");
        foreach (var player in Players.Values.Where(p => p.Mode == PlayerMode.SpikeBall))
        {
            GD.Print($"Setting up spike ball player: {player.Id}");
            PlayerController spikeBallPlayer = SpikeBallScene.Instantiate<PlayerController>();
            PlayerControllers.Add(player.Id, spikeBallPlayer);
            Node2D spikeBallSpawns = parentNode.GetNode<Node2D>("SpawnPoints/SpikeBalls");
            var spikeBallMarkers = spikeBallSpawns.GetChildren().OfType<Marker2D>();
            Marker2D randomSpawn = spikeBallMarkers.ElementAt(new RandomNumberGenerator().RandiRange(0, spikeBallMarkers.Count() - 1));
            spikeBallPlayer.InitPlayerState(player);
            parentNode.AddChild(spikeBallPlayer);
            spikeBallPlayer.GlobalPosition = randomSpawn.GlobalPosition;
        }
        GD.Print("Spike ball players setup complete.");

        StartUI.PreRoundTimerPanel.Visible = false;
        StartUI.PreRoundTimerLabel.Visible = false;
        StartUI.RoundTimerLabel.Visible = true;
        GD.Print("Starting round timer...");
        RoundTimer.Start();
    }

    private void StartPostRound()
    {
        GD.Print("Starting post-round...");
        GD.Print("Clearing player controllers...");
        ClearPlayerControllers();

        GD.Print("Determining if game should end...");
        if (PlayerScores.Values.Any(score => score >= 10))
        {
            EndGame();
        }
        else
        {
            StartPreRound();
        }
    }

    public void EndRound(RoundResult roundResult)
    {
        GD.Print($"Attempting to end round with result: {roundResult}");
        if (roundEnded) return;
        roundEnded = true;
        GD.Print($"Round ended with result: {roundResult}");
        RoundTimer.Stop();
        GD.Print("Calculating scores...");
        var playerId = GetBallPlayer().Id;
        if (roundResult == RoundResult.BallScored)
        {
            PlayerScores[playerId] += PlayerScores.Count - 1;
        }
        else if (roundResult == RoundResult.BallStopped)
        {
            foreach (var playerScore in PlayerScores)
            {
                if (playerScore.Key != playerId)
                    PlayerScores[playerScore.Key] += 1;
            }
        }
        GD.Print("Scores calculated.");
        GD.Print("Updating player scores in UI...");
        StartUI.UpdatePlayerScores(PlayerScores);
        GD.Print("Player scores updated in UI.");
        PostRoundTimer.Start();
    }

    public void EndGame()
    {
        GD.Print("Game Over! Scores: ");
        foreach (var score in PlayerScores)
        {
            GD.Print($"Player {score.Key}: {score.Value}");
        }
        ClearPlayerControllers();
        StartGame();
    }

    private void ClearPlayerControllers()
    {
        PlayerControllers.Values.ToList().ForEach(pc => pc.Free());
        PlayerControllers.Clear();
    }


    public Player GetBallPlayer()
    {
        GD.Print("Getting ball player...");
        return Players.Values.FirstOrDefault(player => player.Mode == PlayerMode.Ball);
    }

    // purely debugging input
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Restart_Game"))
        {
            EndRound(RoundResult.Debug);
        }
    }

    private Player PickBallPlayer()
    {
        if (PlayerHasBeenBallRecently.All(hasBeenBall => hasBeenBall.Value == true))
        {
            PlayerHasBeenBallRecently.Keys.ToList().ForEach(key => PlayerHasBeenBallRecently[key] = false);
        }
        var availablePlayers = PlayerHasBeenBallRecently.Where(hasBeenBall => hasBeenBall.Value == false).ToList();
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        var newBallPlayerId = availablePlayers[rng.RandiRange(0, availablePlayers.Count - 1)].Key;
        var newBallPlayer = Players.Values.FirstOrDefault(p => p.Id == newBallPlayerId);
        newBallPlayer.Mode = PlayerMode.Ball;
        PlayerHasBeenBallRecently[newBallPlayerId] = true;
        Players.Values.Where(p => p.Id != newBallPlayer.Id).ToList().ForEach(p => p.Mode = PlayerMode.SpikeBall);
        EmitSignal(nameof(BallPlayerChanged));
        return newBallPlayer;
    }

    private void RoundEndTimeout()
    {
        PlayerController ballPlayerController = PlayerControllers.Values.FirstOrDefault(pc => pc.PlayerMode == PlayerMode.Ball);
        ballPlayerController?.Explode();
        EmitSignal(SignalName.GoalStopped);
    }

}
