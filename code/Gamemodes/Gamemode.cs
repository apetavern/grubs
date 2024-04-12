namespace Grubs.Gamemodes;

public abstract class Gamemode : Component
{
	public static Gamemode Current { get; set; }

	public bool Started { get; set; }

	public Gamemode()
	{
		Current = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		Initialize();
	}

	internal virtual void Initialize() { }

	internal virtual void Start() { }
}
