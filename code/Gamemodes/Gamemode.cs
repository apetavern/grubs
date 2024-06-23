using Grubs.Common;
using Grubs.Gamemodes.Modes;
using Grubs.Pawn;

namespace Grubs.Gamemodes;

public abstract class Gamemode : Component
{
	public static Gamemode Current { get; set; }
	public static FreeForAllGamemode FFA => Current as FreeForAllGamemode;

	public virtual string GamemodeName => "";
	public virtual string GamemodeShortName => "";

	[Sync] public GameState State { get; set; }
	[Sync] public bool Started { get; set; }
	[Sync] public Guid CameraTarget { get; set; } = Guid.Empty;
	[Sync] public bool TurnIsChanging { get; set; } = false;
	[Sync] public int RoundsPassed { get; set; } = 0;

	public Queue<Grub> DamageQueue { get; set; } = new();
	protected Queue<Player> PlayerTurnQueue { get; set; } = new();

	private int _resolveTries = 0;

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

	internal virtual void Start() 
	{
		Stats.IncrementGamesPlayed( GamemodeShortName );
	}

	internal virtual Task OnRoundPassed()
	{
		RoundsPassed++;

		return Task.CompletedTask;
	}

	protected async Task ApplyDamageQueue()
	{
		while ( DamageQueue.Any() )
		{
			var grub = DamageQueue.Dequeue();
			if ( !grub.IsValid() )
				continue;

			while ( !grub.Resolved && _resolveTries++ <= 20 )
				await GameTask.Delay( 200 );

			_resolveTries = 0;
			grub.Health.ApplyDamage();

			await ShowDamagedGrub( grub );
			await Resolution.UntilWorldResolved( 30 );
		}

		// Remove dead players from turn queue.
		PlayerTurnQueue = new( PlayerTurnQueue.Where( p => p.ShouldHaveTurn ) );
	}

	private async Task ShowDamagedGrub( Grub grub )
	{
		if ( grub.Transform.Position.z < -GrubsConfig.TerrainHeight )
			return;

		CameraTarget = grub.GameObject.Id;
		await GameTask.Delay( 2000 );
		CameraTarget = Guid.Empty;
	}
}
