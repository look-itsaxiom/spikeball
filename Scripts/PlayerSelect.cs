using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class PlayerSelect : Control
{
    public Array<Color> ColorOptions = new Array<Color>
    {
        //Colors.DarkRed,
        Colors.Red,
        //Colors.OrangeRed,
        Colors.Orange,
        Colors.Gold,
        //Colors.Yellow,
        //Colors.Chartreuse,
        //Colors.LimeGreen,
        Colors.Green,
        //Colors.Teal,
        Colors.Blue,
        Colors.Purple,
        Colors.HotPink
    };

    public HBoxContainer ColorOptionsContainer;
    public Control ColorOptionTemplate;
    public VBoxContainer PlayerLanesContainer;
    public Label JoinBanner;
    public Dictionary<int, Color> colorXPositionMap = new Dictionary<int, Color>();
    public Dictionary<int, Control> playerIdToLaneMap = new Dictionary<int, Control>();
    public Dictionary<int, Array<Vector2I>> playerLaneAnchorPoints = new Dictionary<int, Array<Vector2I>>();
    public Dictionary<int, Label> playerNumberLabels = new Dictionary<int, Label>();
    public Dictionary<int, Label> playerReadyLabels = new Dictionary<int, Label>();
    public Dictionary<int, bool> playerReadyStates = new Dictionary<int, bool>();
    public PackedScene PlayerColorSelectorScene = ResourceLoader.Load<PackedScene>("res://Scenes/PlayerColorSelector.tscn");
    public Button StartGameButton;
    public const int MIN_PLAYERS = 2;

    public override void _Ready()
    {
        ColorOptionsContainer = GetNode<HBoxContainer>("PlayerColorSelect/ColorOptions");
        ColorOptionTemplate = GetNode<Control>("PlayerColorSelect/ColorOptions/ColorOption");
        JoinBanner = GetNode<Label>("JoinBanner");
        PlayerLanesContainer = GetNode<VBoxContainer>("PlayerColorSelect/PlayerLanes");
        StartGameButton = GetNode<Button>("StartGameButton");

        InitializeColorOptions();

        JoinBanner.Visible = true;
        StartGameButton.Visible = false;
        StartGameButton.Pressed += StartGame;
        
        Timer joinBannerTimer = new Timer();
        joinBannerTimer.WaitTime = 0.75f;
        joinBannerTimer.OneShot = false;
        joinBannerTimer.Autostart = true;
        joinBannerTimer.Timeout += () =>
        {
            JoinBanner.Visible = !JoinBanner.Visible;
        };
        AddChild(joinBannerTimer);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventJoypadButton joypadButton && joypadButton.IsPressed() && joypadButton.ButtonIndex == JoyButton.Start)
        {
            GD.Print("Start button pressed, Device: " + joypadButton.Device);
            if (!PlayerRegistrar.Instance.IsDeviceRegistered(joypadButton.Device))
            {
                AddPlayer(joypadButton.Device);
            }
        }
        base._Input(@event);
    }

    public void AddPlayer(int deviceId)
    {
        // Register device with player registrar to get player ID back
        var playerRegistration = PlayerRegistrar.Instance.RegisterDevice(deviceId);
        if (playerRegistration == null) return;
        // Determine which lane is the next available
        int playerLaneIndex = playerIdToLaneMap.Count + 1;
        if (playerLaneIndex <= 4)
        {
            // Get the PlayerLane for the player
            var playerLane = PlayerLanesContainer.GetChild(playerLaneIndex - 1);
            if (playerLane == null) return;
            playerIdToLaneMap[playerRegistration.Id] = playerLane as Control;

            // Determine anchor points for that lane
            var anchorPoints = playerLaneAnchorPoints[playerLaneIndex];

            // Create a PlayerColorSelector instance for that player and pass in the anchor points and player ID
            var playerColorSelector = PlayerColorSelectorScene.Instantiate<PlayerColorSelector>();
            playerColorSelector.InitializePlayerColorSelector(anchorPoints, playerRegistration.Id);
            playerLane.AddChild(playerColorSelector);
            playerColorSelector.ColorSelected += SetPlayerColor;
            playerColorSelector.PlayerReady += OnPlayerReady;
            
            // Initialize player ready state
            playerReadyStates[playerRegistration.Id] = false;
        }
    }

    private void SetPlayerColor(int playerNumber, Vector2I position)
    {
        if (colorXPositionMap.TryGetValue(position.X, out var color))
        {
            PlayerRegistrar.Instance.SetPlayerColor(playerNumber, color);
        }
    }

    private void OnPlayerReady(int playerNumber, bool isReady)
    {
        playerReadyStates[playerNumber] = isReady;
        
        // Update the ready label for this player
        if (playerReadyLabels.TryGetValue(playerNumber, out var readyLabel))
        {
            readyLabel.Text = isReady ? "READY" : "";
        }
        
        UpdateStartGameButton();
    }

    private void UpdateStartGameButton()
    {
        // Check if we have at least MIN_PLAYERS and all joined players are ready
        var joinedPlayers = playerReadyStates.Count;
        var readyPlayers = playerReadyStates.Count(kvp => kvp.Value);
        
        bool canStartGame = joinedPlayers >= MIN_PLAYERS && readyPlayers == joinedPlayers;
        
        StartGameButton.Visible = canStartGame;
        
        if (canStartGame)
        {
            JoinBanner.Visible = false;
        }
    }

    private void StartGame()
    {
        // Transition to the game scene
        GetTree().ChangeSceneToFile("res://Scenes/Core_Spikeball.tscn");
    }


    private void InitializeColorOptions()
    {
        foreach (var color in ColorOptions)
        {
            var colorOption = ColorOptionTemplate.Duplicate() as Control;
            var colorRect = colorOption.GetNode<ColorRect>("ColorRect");
            colorRect.Color = color;
            ColorOptionsContainer.AddChild(colorOption);
        }
        ColorOptionTemplate.Free();

        CallDeferred(nameof(MapColorPositions));
    }

    private void MapColorPositions()
    {
        foreach (Control child in ColorOptionsContainer.GetChildren())
        {
            colorXPositionMap.Add((int)child.Position.X, child.GetNode<ColorRect>("ColorRect").Color);
        }

        InitializePlayerLanes();
    }


    private void InitializePlayerLanes()
    {
        for (int i = 0; i < PlayerLanesContainer.GetChildCount(); i++)
        {
            if (PlayerLanesContainer.GetChild(i) is Control playerLane)
            {
                int playerLaneNumber = i + 1;
                playerNumberLabels[playerLaneNumber] = playerLane.GetNode<Label>($"Player{playerLaneNumber}LabelContainer/Player{playerLaneNumber}Label");
                playerReadyLabels[playerLaneNumber] = playerLane.GetNode<Label>($"Player{playerLaneNumber}LabelContainer/Player{playerLaneNumber}ReadyLabel");
                var anchorPoints = new Array<Vector2I>();
                foreach (Control child in ColorOptionsContainer.GetChildren())
                {
                    if (child is Control colorOption)
                    {
                        // Calculate the center of the color option horizontally
                        float centerX = colorOption.GetPosition().X + colorOption.Size.X / 2 - 8;
                        // Calculate the center of the player lane vertically
                        float centerY = playerLane.GetPosition().Y + playerLane.Size.Y / 2 - 8;

                        Vector2I anchorPoint = new Vector2I((int)centerX, (int)centerY);
                        GD.Print($"Anchor point for Player {playerLaneNumber}: {anchorPoint}");
                        anchorPoints.Add(anchorPoint);
                    }
                }
                playerLaneAnchorPoints.Add(playerLaneNumber, anchorPoints);
            }
        }
        GD.Print("Player lanes anchor points initialized.");
        GD.Print(playerLaneAnchorPoints.Count + " player lanes anchor points found.");

    }
}
