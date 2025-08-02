using Godot;
using Godot.Collections;
using System;

public partial class PlayerColorSelector : TextureRect
{
    public Array<Vector2I> AnchorPoints;
    public int PlayerNumber;
    public bool isReady = false;
    public bool isMoving = false;
    private int currentAnchorIndex = 0;

    [Signal]
    public delegate void ColorSelectedEventHandler(int playerNumber, Vector2I position);

    [Signal]
    public delegate void PlayerReadyEventHandler(int playerNumber, bool isReady);
    private float moveCooldown = 0.25f;
    private float moveTimer = 0f;

    public override void _Process(double delta)
    {
        if (moveTimer > 0f)
            moveTimer -= (float)delta;
    }

    public void InitializePlayerColorSelector(Array<Vector2I> anchorPoints, int playerNumber)
    {
        AnchorPoints = anchorPoints;
        currentAnchorIndex = 0;
        PlayerNumber = playerNumber;

        // Defer setting the initial position to avoid layout issues
        CallDeferred(nameof(SetInitialPosition));
    }

    private void SetInitialPosition()
    {
        if (AnchorPoints.Count > 0)
        {
            Position = AnchorPoints[currentAnchorIndex];
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Handle color selection movement
        if (isReady || isMoving || moveTimer > 0f) return;

        if (Input.IsActionJustPressed($"Move_Player{PlayerNumber}_Right"))
        {
            if (AnchorPoints.Count == 0) return;
            var nextAnchorIndex = currentAnchorIndex + 1;
            if (nextAnchorIndex >= AnchorPoints.Count) return;

            currentAnchorIndex = nextAnchorIndex;
            Vector2 newPosition = AnchorPoints[currentAnchorIndex];
            MoveToPosition(newPosition);
        }
        else if (Input.IsActionJustPressed($"Move_Player{PlayerNumber}_Left"))
        {
            if (AnchorPoints.Count == 0) return;
            var nextAnchorIndex = currentAnchorIndex - 1;
            if (nextAnchorIndex < 0) return;

            currentAnchorIndex = nextAnchorIndex;
            Vector2 newPosition = AnchorPoints[currentAnchorIndex];
            MoveToPosition(newPosition);
        }

        if (Input.IsActionJustPressed($"Boost_Player{PlayerNumber}"))
        {
            isReady = !isReady;
            EmitSignal(SignalName.PlayerReady, PlayerNumber, isReady);
            GD.Print($"Player {PlayerNumber} is now {(isReady ? "ready" : "not ready")}.");
            if (isReady)
            {
                // I just want to change the color to a slightly grayed out version to indicate readiness
                Modulate = new Color(0.5f, 0.5f, 0.5f); // Change color to gray
            }
            else
            {
                Modulate = new Color(0, 0, 0); // Reset color to black
            }
        }
    }

    private void MoveToPosition(Vector2 newPosition)
    {
        moveTimer = moveCooldown;

        Tween tween = CreateTween();
        tween.TweenProperty(this, "position", newPosition, 0.1f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        tween.Finished += () =>
        {
            isMoving = false;
        };
        tween.Play();
        isMoving = true;
        EmitSignal(SignalName.ColorSelected, PlayerNumber, (Vector2I)newPosition);
    }
}
