using Godot;
using System;

public partial class NetworkLobby : Control
{
    private LineEdit addressInput;
    private LineEdit portInput;
    private Button hostButton;
    private Button joinButton;
    private Button backButton;
    private Label statusLabel;
    private VBoxContainer lobbyPlayerList;
    
    public override void _Ready()
    {
        // Get UI nodes
        addressInput = GetNode<LineEdit>("VBoxContainer/ConnectionSettings/AddressInput");
        portInput = GetNode<LineEdit>("VBoxContainer/ConnectionSettings/PortInput");
        hostButton = GetNode<Button>("VBoxContainer/Buttons/HostButton");
        joinButton = GetNode<Button>("VBoxContainer/Buttons/JoinButton");
        backButton = GetNode<Button>("VBoxContainer/Buttons/BackButton");
        statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
        lobbyPlayerList = GetNode<VBoxContainer>("VBoxContainer/LobbyPlayerList");
        
        // Set default values
        portInput.Text = NetworkManager.DEFAULT_PORT.ToString();
        addressInput.Text = "127.0.0.1";
        
        // Connect button signals
        hostButton.Pressed += OnHostButtonPressed;
        joinButton.Pressed += OnJoinButtonPressed;
        backButton.Pressed += OnBackButtonPressed;
        
        // Connect network signals
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.PlayerConnected += OnPlayerConnected;
            NetworkManager.Instance.PlayerDisconnected += OnPlayerDisconnected;
            NetworkManager.Instance.ConnectionFailed += OnConnectionFailed;
            NetworkManager.Instance.ServerDisconnected += OnServerDisconnected;
        }
        
        UpdateUI();
    }
    
    private void OnHostButtonPressed()
    {
        int port = int.Parse(portInput.Text);
        Error result = NetworkManager.Instance.CreateHost(port);
        
        if (result == Error.Ok)
        {
            statusLabel.Text = $"Hosting on port {port}";
            statusLabel.Modulate = Colors.Green;
            UpdateUI();
            
            // Allow host to play locally (hotseat)
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }
        else
        {
            statusLabel.Text = $"Failed to host: {result}";
            statusLabel.Modulate = Colors.Red;
        }
    }
    
    private void OnJoinButtonPressed()
    {
        string address = addressInput.Text;
        int port = int.Parse(portInput.Text);
        
        Error result = NetworkManager.Instance.JoinHost(address, port);
        
        if (result == Error.Ok)
        {
            statusLabel.Text = $"Connecting to {address}:{port}...";
            statusLabel.Modulate = Colors.Yellow;
            UpdateUI();
        }
        else
        {
            statusLabel.Text = $"Failed to connect: {result}";
            statusLabel.Modulate = Colors.Red;
        }
    }
    
    private void OnBackButtonPressed()
    {
        NetworkManager.Instance.CloseConnection();
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }
    
    private void OnPlayerConnected(int peerId)
    {
        statusLabel.Text = $"Player {peerId} connected";
        statusLabel.Modulate = Colors.Green;
        UpdatePlayerList();
    }
    
    private void OnPlayerDisconnected(int peerId)
    {
        statusLabel.Text = $"Player {peerId} disconnected";
        statusLabel.Modulate = Colors.Orange;
        UpdatePlayerList();
    }
    
    private void OnConnectionFailed()
    {
        statusLabel.Text = "Connection failed";
        statusLabel.Modulate = Colors.Red;
        UpdateUI();
    }
    
    private void OnServerDisconnected()
    {
        statusLabel.Text = "Server disconnected";
        statusLabel.Modulate = Colors.Red;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        bool isConnected = NetworkManager.Instance.IsNetworkActive;
        
        addressInput.Editable = !isConnected;
        portInput.Editable = !isConnected;
        hostButton.Disabled = isConnected;
        joinButton.Disabled = isConnected;
        
        UpdatePlayerList();
    }
    
    private void UpdatePlayerList()
    {
        // Clear existing list
        foreach (Node child in lobbyPlayerList.GetChildren())
        {
            child.QueueFree();
        }
        
        if (!NetworkManager.Instance.IsNetworkActive)
        {
            return;
        }
        
        // Add connected players
        var players = NetworkManager.Instance.GetNetworkPlayers();
        foreach (var player in players.Values)
        {
            Label playerLabel = new Label();
            playerLabel.Text = $"Player {player.PeerId}" + (player.IsLocal ? " (You)" : "");
            lobbyPlayerList.AddChild(playerLabel);
        }
    }
    
    public override void _ExitTree()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.PlayerConnected -= OnPlayerConnected;
            NetworkManager.Instance.PlayerDisconnected -= OnPlayerDisconnected;
            NetworkManager.Instance.ConnectionFailed -= OnConnectionFailed;
            NetworkManager.Instance.ServerDisconnected -= OnServerDisconnected;
        }
    }
}
