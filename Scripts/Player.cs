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
    public enum PlayerColor
    {
        Pink,
        Purple,
        Green,
        Blue
    }

    public class PlayerData
    {
        public static Dictionary<PlayerColor, Color> PlayerColors = new Dictionary<PlayerColor, Color>
        {
            { PlayerColor.Pink, Colors.HotPink },
            { PlayerColor.Purple, Colors.Purple },
            { PlayerColor.Green, Colors.Green },
            { PlayerColor.Blue, Colors.Blue }
        };

        public static Dictionary<int, PlayerColor> PlayerColorMap = new Dictionary<int, PlayerColor>
        {
            { 4, PlayerColor.Pink },
            { 2, PlayerColor.Purple },
            { 3, PlayerColor.Green },
            { 1, PlayerColor.Blue }
        };

        public const int BALL_SPEED = 100;
        public const int SPIKEBALL_SPEED = 75;
        public const int BOOST_SPEED = 300;
        public const float BOOST_DURATION = 0.2f;
        public const float BOOST_COOLDOWN = 2.0f;
    }

    public partial class Player : Resource
    {
        public PlayerColor Color { get; set; }
        public int Id { get; set; }
        public PlayerMode Mode { get; set; }
    }
}