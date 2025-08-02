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
    }

    public partial class PlayerRegistration : Resource
    {
        public Color Color { get; set; }
        public int Id { get; set; }
        public int Device { get; set; }
        public PlayerRegistration(int id, int device, Color color)
        {
            Color = color;
            Id = id;
            Device = device;
        }
    }
}