using Godot;
using Godot.Collections;
using Players;
using System;
using System.Linq;

public partial class PlayerRegistrar : Node
{
    public static PlayerRegistrar Instance { get; private set; }
    public Array<PlayerRegistration> RegisteredPlayers { get; private set; } = new Array<PlayerRegistration>();
    public Array<string> ActionNames { get; private set; } = new Array<string>
    {
        "Move_Player{Id}_Up",
        "Move_Player{Id}_Down",
        "Move_Player{Id}_Left",
        "Move_Player{Id}_Right",
        "Boost_Player{Id}"
    };

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr("Multiple instances of PlayerRegistrar detected!");
            QueueFree();
        }
    }

    public bool IsDeviceRegistered(int device)
    {
        foreach (var player in RegisteredPlayers)
        {
            if (player.Device == device)
            {
                return true;
            }
        }
        return false;
    }

    public Array<PlayerRegistration> GetRegisteredPlayers()
    {
        return RegisteredPlayers;
    }

    internal PlayerRegistration RegisterDevice(int deviceId)
    {
        // Check if the device is already registered
        if (IsDeviceRegistered(deviceId))
        {
            GD.Print($"Device {deviceId} is already registered.");
            return null;
        }

        int newPlayerId = RegisteredPlayers.Count + 1;

        foreach (var actionName in ActionNames)
        {
            // Create a unique action name for the new player
            string uniqueActionName = actionName.Replace("{Id}", newPlayerId.ToString());
            if (!InputMap.HasAction(uniqueActionName))
            {
                InputMap.AddAction(uniqueActionName, 0.2f);
                AddActionEvent(actionName, uniqueActionName, deviceId);
            }
            else
            {
                GD.Print($"Action {uniqueActionName} already exists.");
            }
        }

        // Create a new PlayerRegistration with a new ID and the given device ID

        var playerRegistration = new PlayerRegistration(newPlayerId, deviceId, Colors.White);
        RegisteredPlayers.Add(playerRegistration);
        GD.Print($"Registered new player with ID {newPlayerId} for device {deviceId}.");
        return playerRegistration;
    }

    public void UnregisterDevice(int deviceId)
    {
        var playerToRemove = RegisteredPlayers.FirstOrDefault(player => player.Device == deviceId);
        if (playerToRemove != null)
        {
            RegisteredPlayers.Remove(playerToRemove);
            // Remove the actions associated with this player
            foreach (var actionName in ActionNames)
            {
                string uniqueActionName = actionName.Replace("{Id}", playerToRemove.Id.ToString());
                if (InputMap.HasAction(uniqueActionName))
                {
                    InputMap.EraseAction(uniqueActionName);
                }
            }

            GD.Print($"Unregistered player with ID {playerToRemove.Id} for device {deviceId}.");
        }
        else
        {
            GD.Print($"No player registered for device {deviceId}.");
        }
    }

    private void AddActionEvent(string actionName, string uniqueActionName, int deviceId)
    {
        switch (actionName)
        {
            case "Move_Player{Id}_Up":
                InputMap.ActionAddEvent(uniqueActionName, new InputEventJoypadMotion { ResourceLocalToScene = false, Axis = JoyAxis.LeftY, AxisValue = -1, Device = deviceId });
                break;
            case "Move_Player{Id}_Down":
                InputMap.ActionAddEvent(uniqueActionName, new InputEventJoypadMotion { ResourceLocalToScene = false, Axis = JoyAxis.LeftY, AxisValue = 1, Device = deviceId });
                break;
            case "Move_Player{Id}_Left":
                InputMap.ActionAddEvent(uniqueActionName, new InputEventJoypadMotion { ResourceLocalToScene = false, Axis = JoyAxis.LeftX, AxisValue = -1, Device = deviceId });
                break;
            case "Move_Player{Id}_Right":
                InputMap.ActionAddEvent(uniqueActionName, new InputEventJoypadMotion { ResourceLocalToScene = false, Axis = JoyAxis.LeftX, AxisValue = 1, Device = deviceId });
                break;
            case "Boost_Player{Id}":
                InputMap.ActionAddEvent(uniqueActionName, new InputEventJoypadButton { ButtonIndex = JoyButton.A, Pressed = false, ResourceLocalToScene = false, Device = deviceId });
                break;
        }

        GD.Print($"Added action: {uniqueActionName} for device {deviceId}");
    }

    internal void SetPlayerColor(int playerNumber, Color color)
    {
        var player = RegisteredPlayers.FirstOrDefault(p => p.Id == playerNumber);
        if (player != null)
        {
            player.Color = color;
            GD.Print($"Set color for Player {playerNumber} to {color}");
        }
    }
}
