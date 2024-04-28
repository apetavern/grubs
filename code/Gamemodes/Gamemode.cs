using Grubs.Common;
using Grubs.Gamemodes.Modes;
using Grubs.Pawn;

namespace Grubs.Gamemodes;

public abstract class Gamemode : Component
{
	public static Gamemode Current { get; set; }
	public static FreeForAllGamemode FFA => Current as FreeForAllGamemode;

	public virtual string GamemodeName => "";

	[Sync] public GameState State { get; set; }
	[Sync] public bool Started { get; set; }
	[Sync] public Guid CameraTarget { get; set; } = Guid.Empty;
	[Sync] public bool TurnIsChanging { get; set; } = false;

	public Queue<Grub> DamageQueue { get; set; } = new();

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

	protected async Task ApplyDamageQueue()
	{
		while ( DamageQueue.Any() )
		{
			var grub = DamageQueue.Dequeue();
			if ( !grub.IsValid() )
				continue;

			while ( !grub.Resolved )
				await GameTask.Delay( 200 );

			grub.Health.ApplyDamage();

			await ShowDamagedGrub( grub );
			await Resolution.UntilWorldResolved( 30 );
		}
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
