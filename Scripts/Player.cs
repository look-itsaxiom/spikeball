using Godot;
using Godot.Collections;

namespace Players
{
    public enum PlayerMode
    {
        Ball,
        SpikeBall,
        Setup
    }

    public class PlayerData
    {
        public const int BALL_SPEED = 100;
        public const int SPIKEBALL_SPEED = 75;
        public const int BOOST_SPEED = 300;
        public const float BOOST_DURATION = 0.2f;
        public const float BOOST_COOLDOWN = 2.0f;
    }

    public partial class Player : Resource
    {
        public Color Color { get; set; }
        public int Id { get; set; }
        public PlayerMode Mode { get; set; }
        public int PeerId { get; set; } = -1; // Network peer ID, -1 for local-only
        public bool IsLocal { get; set; } = true; // Whether this player is controlled locally
    }

    public partial class PlayerRegistration : Resource
    {
        public Color Color { get; set; }
        public int Id { get; set; }
        public int Device { get; set; }
        public int PeerId { get; set; } = -1; // Network peer ID
        public bool IsLocal { get; set; } = true; // Whether this player is local
        
        public PlayerRegistration(int id, int device, Color color, int peerId = -1, bool isLocal = true)
        {
            Color = color;
            Id = id;
            Device = device;
            PeerId = peerId;
            IsLocal = isLocal;
        }
    }
}