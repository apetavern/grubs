using Grubs.Common;
using Grubs.Extensions;
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

	// Queue<Grub>
	[Sync] public NetList<Guid> DamageQueue { get; set; } = new();
	// Queue<Player>
	[Sync] protected NetList<Guid> PlayerTurnQueue { get; set; } = new();

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
		RoundsPassed = 0;
		Stats.IncrementGamesPlayed( GamemodeShortName );
		Resolution.ClearForceResolved();
	}

	internal virtual Task OnRoundPassed()
	{
		RoundsPassed++;

		return Task.CompletedTask;
	}

	protected async Task ApplyDamageQueue()
	{
		foreach ( var grub in Scene.GetAllComponents<Grub>().Where( g => g.Player.GameObject is null ) )
		{
			grub.Health.TakeDamage( GrubsDamageInfo.FromDisconnect() );
		}

		while ( DamageQueue.Any() )
		{
			var grub = DamageQueue[0].ToComponent<Grub>();
			DamageQueue.RemoveAt( 0 );
			if ( !grub.IsValid() )
				continue;

			while ( !grub.Resolved && _resolveTries++ <= 20 )
				await GameTask.DelayRealtime( 200 );

			_resolveTries = 0;
			grub.Health.ApplyDamage();

			await ShowDamagedGrub( grub );
			await Resolution.UntilWorldResolved( 30 );
		}

		// Remove dead players from turn queue.
		for ( int i = 0; i < PlayerTurnQueue.Count; i++ )
		{
			var player = PlayerTurnQueue[i];
			if ( !player.ToComponent<Player>()?.ShouldHaveTurn ?? false )
			{
				PlayerTurnQueue.Remove( player );
				i--;
			}
		}
	}

	private async Task ShowDamagedGrub( Grub grub )
	{
		if ( grub.Transform.Position.z < -GrubsConfig.TerrainHeight )
			return;

		CameraTarget = grub.GameObject.Id;
		await GameTask.DelayRealtime( 2000 );
		CameraTarget = Guid.Empty;
	}
}
