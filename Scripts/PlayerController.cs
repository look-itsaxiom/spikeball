using Godot;
using Players;
using System;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public PlayerMode PlayerMode;

	[Export]
	public int Speed;

	public Sprite2D PlayerSprite;
	public CpuParticles2D PlayerDieEffect;
	public TextureProgressBar BoostCooldownBar;

	[Export]
	public int PlayerId;
	
	public int PeerId = -1; // Network peer ID
	public bool IsLocal = true; // Whether this player is controlled locally

	private bool hasExploded = false;
	private bool isBoosting = false;
	private Vector2 boostDirection;
	private float boostTimer = 0.0f;
	private float boostCooldownTimer = 0.0f;
	
	// Network sync variables
	private Vector2 networkPosition;
	private Vector2 networkVelocity;
	private bool useNetworkState = false;
	private float networkSyncTimer = 0.0f;
	private const float NETWORK_SYNC_RATE = 0.05f; // Sync 20 times per second

	public override void _PhysicsProcess(double delta)
	{
		if (PlayerMode == PlayerMode.Ball)
		{
			BoostCooldownBar.Visible = boostCooldownTimer > 0.0f;
			BoostCooldownBar.Value = boostCooldownTimer;
		}

		if (boostCooldownTimer > 0.0f)
			boostCooldownTimer -= (float)delta;
		
		// Update network sync timer
		if (networkSyncTimer > 0.0f)
			networkSyncTimer -= (float)delta;

		// For network players, use interpolated network state
		if (!IsLocal && useNetworkState)
		{
			// Interpolate to network position for smooth movement
			GlobalPosition = GlobalPosition.Lerp(networkPosition, 0.25f);
			Velocity = networkVelocity;
			HandleMove(Velocity, delta);
			return;
		}

		if (isBoosting)
		{
			Velocity = boostDirection * PlayerData.BOOST_SPEED;
			boostTimer -= (float)delta;
			if (boostTimer <= 0.0f)
			{
				isBoosting = false;
			}
		}
		else
		{
			Vector2 velocity = Velocity;
			velocity.X = Input.GetActionStrength($"Move_Player{PlayerId}_Right") - Input.GetActionStrength($"Move_Player{PlayerId}_Left");
			velocity.Y = Input.GetActionStrength($"Move_Player{PlayerId}_Down") - Input.GetActionStrength($"Move_Player{PlayerId}_Up");
			Velocity = velocity.Normalized() * Speed;
			if (PlayerMode == PlayerMode.Ball && Input.IsActionJustPressed($"Boost_Player{PlayerId}") && boostCooldownTimer <= 0.0f)
			{
				isBoosting = true;
				boostDirection = Velocity.Normalized();
				boostTimer = PlayerData.BOOST_DURATION;
				boostCooldownTimer = PlayerData.BOOST_COOLDOWN;
			}
		}

		HandleMove(Velocity, delta);
		
		// Sync position and velocity over network if this is a local player (at a limited rate)
		if (IsLocal && NetworkManager.Instance != null && NetworkManager.Instance.IsNetworkActive && networkSyncTimer <= 0.0f)
		{
			Rpc(nameof(SyncPlayerState), GlobalPosition, Velocity, isBoosting, boostCooldownTimer);
			networkSyncTimer = NETWORK_SYNC_RATE;
		}
	}

	private void HandleMove(Vector2 velocity, double delta)
	{
		Velocity = velocity;
		var collision = MoveAndCollide(velocity * (float)delta);

		if (collision != null)
		{
			if (collision.GetCollider() is PlayerController otherPlayer)
			{
				if (otherPlayer.PlayerMode == PlayerMode.Ball)
				{
					otherPlayer.Velocity = new Vector2(Speed, 0).Rotated((float)GD.Randf() * Mathf.Tau);
					otherPlayer.Explode();
				}
				else if (this.PlayerMode == PlayerMode.Ball && otherPlayer.PlayerMode == PlayerMode.SpikeBall)
				{
					Explode();
				}
				else
				{
					Velocity = Velocity.Bounce(collision.GetNormal());
					MoveAndCollide(Velocity * (float)delta);
				}
			}
		}
	}

	public void Explode()
	{
		if (hasExploded) return;
		hasExploded = true;
		PlayerSprite.Visible = false;
		PlayerDieEffect.Emitting = true;
		GetParent<SpikeballManager>().EmitSignal("GoalStopped");
	}

	public void InitPlayerState(Player playerState)
	{
		PlayerId = playerState.Id;
		PlayerMode = playerState.Mode;
		PeerId = playerState.PeerId;
		IsLocal = playerState.IsLocal;
		
		PlayerSprite = GetNode<Sprite2D>("PlayerSprite");
		PlayerDieEffect = GetNode<CpuParticles2D>("PlayerDieEffect");
		Color color = playerState.Color;
		PlayerDieEffect.Color = color;
		PlayerSprite.Modulate = color;

		switch (PlayerMode)
		{
			case PlayerMode.Ball:
				Speed = PlayerData.BALL_SPEED;
				BoostCooldownBar = GetNode<TextureProgressBar>("BoostCooldownBar");
				BoostCooldownBar.TintProgress = color;
				break;
			case PlayerMode.SpikeBall:
				Speed = PlayerData.SPIKEBALL_SPEED;
				GetParent<SpikeballManager>().GoalScored += Explode;
				break;
		}
		
		GD.Print($"Initialized player {PlayerId} (Peer: {PeerId}, Local: {IsLocal})");
	}

	public override void _ExitTree()
	{
		GetParent<SpikeballManager>().GoalScored -= Explode;
		base._ExitTree();
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void SyncPlayerState(Vector2 position, Vector2 velocity, bool boosting, float boostCd)
	{
		networkPosition = position;
		networkVelocity = velocity;
		isBoosting = boosting;
		boostCooldownTimer = boostCd;
		useNetworkState = true;
	}

}
