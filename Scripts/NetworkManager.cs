using Godot;
using Godot.Collections;
using Players;
using System;
using System.Linq;

public partial class NetworkManager : Node
{
    public static NetworkManager Instance { get; private set; }
    
    public enum NetworkMode
    {
        Local,
        Host,
        Client
    }
    
    public NetworkMode CurrentMode { get; private set; } = NetworkMode.Local;
    public bool IsNetworkActive => CurrentMode != NetworkMode.Local;
    public bool IsHost => CurrentMode == NetworkMode.Host;
    
    private ENetMultiplayerPeer peer;
    private System.Collections.Generic.Dictionary<int, PlayerNetworkInfo> networkPlayers = new System.Collections.Generic.Dictionary<int, PlayerNetworkInfo>();
    
    public const int DEFAULT_PORT = 7777;
    public const int MAX_PLAYERS = 4;
    
    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId);
    
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    
    [Signal]
    public delegate void ConnectionFailedEventHandler();
    
    [Signal]
    public delegate void ServerDisconnectedEventHandler();
    
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr("Multiple instances of NetworkManager detected!");
            QueueFree();
            return;
        }
        
        // Set up multiplayer signals
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }
    
    public Error CreateHost(int port = DEFAULT_PORT)
    {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port, MAX_PLAYERS);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to create server: {error}");
            return error;
        }
        
        Multiplayer.MultiplayerPeer = peer;
        CurrentMode = NetworkMode.Host;
        
        GD.Print($"Server created on port {port}");
        
        // Register the host as a local player
        RegisterLocalPlayer(1); // Host always has peer ID 1
        
        return Error.Ok;
    }
    
    public Error JoinHost(string address, int port = DEFAULT_PORT)
    {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address, port);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to connect to server: {error}");
            return error;
        }
        
        Multiplayer.MultiplayerPeer = peer;
        CurrentMode = NetworkMode.Client;
        
        GD.Print($"Connecting to {address}:{port}");
        
        return Error.Ok;
    }
    
    public void CloseConnection()
    {
        if (peer != null)
        {
            peer.Close();
            peer = null;
        }
        
        networkPlayers.Clear();
        CurrentMode = NetworkMode.Local;
        Multiplayer.MultiplayerPeer = null;
        
        GD.Print("Network connection closed");
    }
    
    private void OnPeerConnected(long peerId)
    {
        GD.Print($"Peer connected: {peerId}");
        EmitSignal(SignalName.PlayerConnected, (int)peerId);
    }
    
    private void OnPeerDisconnected(long peerId)
    {
        GD.Print($"Peer disconnected: {peerId}");
        
        if (networkPlayers.ContainsKey((int)peerId))
        {
            networkPlayers.Remove((int)peerId);
        }
        
        EmitSignal(SignalName.PlayerDisconnected, (int)peerId);
    }
    
    private void OnConnectedToServer()
    {
        GD.Print("Successfully connected to server");
        
        // Register this client's local player
        int myPeerId = Multiplayer.GetUniqueId();
        RegisterLocalPlayer(myPeerId);
    }
    
    private void OnConnectionFailed()
    {
        GD.PrintErr("Connection to server failed");
        CloseConnection();
        EmitSignal(SignalName.ConnectionFailed);
    }
    
    private void OnServerDisconnected()
    {
        GD.Print("Server disconnected");
        CloseConnection();
        EmitSignal(SignalName.ServerDisconnected);
    }
    
    private void RegisterLocalPlayer(int peerId)
    {
        if (!networkPlayers.ContainsKey(peerId))
        {
            networkPlayers[peerId] = new PlayerNetworkInfo
            {
                PeerId = peerId,
                IsLocal = true
            };
            GD.Print($"Registered local player with peer ID: {peerId}");
        }
    }
    
    public int GetLocalPeerId()
    {
        if (!IsNetworkActive)
            return -1;
        
        return Multiplayer.GetUniqueId();
    }
    
    public bool IsPlayerLocal(int peerId)
    {
        if (!IsNetworkActive)
            return true; // In local mode, all players are local
        
        return peerId == Multiplayer.GetUniqueId();
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RegisterNetworkPlayer(int playerId, int peerId, Color color)
    {
        if (!networkPlayers.ContainsKey(peerId))
        {
            networkPlayers[peerId] = new PlayerNetworkInfo
            {
                PeerId = peerId,
                PlayerId = playerId,
                Color = color,
                IsLocal = IsPlayerLocal(peerId)
            };
            GD.Print($"Registered network player {playerId} for peer {peerId}");
        }
    }
    
    public System.Collections.Generic.Dictionary<int, PlayerNetworkInfo> GetNetworkPlayers()
    {
        return networkPlayers;
    }
}

public class PlayerNetworkInfo
{
    public int PeerId { get; set; }
    public int PlayerId { get; set; }
    public Color Color { get; set; }
    public bool IsLocal { get; set; }
}
