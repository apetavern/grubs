namespace Grubs.Systems.Wind;

[Title( "Grubs - Wind System" ), Category( "Grubs/Systems" )]
public sealed class WindSystem : Component
{
    public static WindSystem Instance { get; private set; }

    /// <summary>
    /// Current wind force. Negative = left, positive = right. Range: -1 to 1.
    /// </summary>
    [Sync( SyncFlags.FromHost )]
    public float WindForce { get; private set; }

    /// <summary>
    /// Maximum wind speed, in the same units as arc trace wind force.
    /// </summary>
    [Property] public float MaxWindForce { get; set; } = 3f;

    protected override void OnStart()
    {
        Instance = this;
    }

    /// <summary>
    /// Picks a new random wind value. Should only be called on host.
    /// </summary>
    public void Randomize()
    {
        WindForce = Game.Random.Float( -MaxWindForce, MaxWindForce );
    }
}
