namespace Grubs;

public partial class Grub
{
	public DeathReason DeathReason { get; set; }

	public Task DeathTask { get; set; }

	[Net]
	public bool HasBeenDamaged { get; set; }

	[Net]
	public bool ShouldTakeDamage { get; set; }

	[Net]
	public int TotalDamageTaken { get; set; }

	public Queue<DamageInfo> DamageQueue { get; set; } = new();

	public override void TakeDamage( DamageInfo info )
	{
		if ( !Game.IsServer )
			return;

		if ( !ShouldTakeDamage )
		{
			if ( IsTurn && GamemodeSystem.Instance is FreeForAll )
				GamemodeSystem.Instance.UseTurn( false );

			DamageQueue.Enqueue( info );
			HasBeenDamaged = true;
			return;
		}

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( Health <= 0 || LifeState != LifeState.Alive )
			return;

		Health -= info.Damage;

		if ( Health <= 0 )
		{
			Health = 0;
			OnKilled();
		}
	}

	public bool ApplyDamage()
	{
		if ( !HasBeenDamaged )
			return false;

		ShouldTakeDamage = true;

		var totalDamage = 0f;
		var damageInfo = new List<DamageInfo>();
		while ( DamageQueue.TryDequeue( out var dmgInfo ) )
		{
			damageInfo.Add( dmgInfo );
			totalDamage += dmgInfo.Damage;
		}

		var dead = false;
		if ( totalDamage >= Health )
		{
			dead = true;
			DeathReason = DeathReason.FindReason( this, damageInfo );
		}

		TotalDamageTaken = totalDamage.FloorToInt();

		TakeDamage( DamageInfo.Generic( Math.Min( totalDamage, Health ) ) );

		ShouldTakeDamage = false;
		HasBeenDamaged = false;
		return dead;
	}

	public override void OnKilled()
	{
		if ( LifeState is LifeState.Dying or LifeState.Dead )
			return;

		// Force a holster of the active weapon and
		// set it to null immediately since Simulate() won't handle it.
		Player.Inventory.ActiveWeapon?.Holster( this );
		Player.Inventory.SetActiveWeapon( null, true );

		DeathTask = Die();
	}

	private async Task Die()
	{
		if ( DeathReason.FromKillTrigger )
		{
			FinishDie();
			return;
		}

		await GameTask.Delay( 200 );
		LifeState = LifeState.Dying;
		var plunger = new ModelEntity( "models/tools/dynamiteplunger/dynamiteplunger.vmdl" )
		{
			Position = Facing == -1 ? Position - new Vector3( 30, 0, 0 ) : Position
		};
		await GameTask.Delay( 1025 );

		ExplosionHelper.Explode( Position, this, 50f );
		PlayDeathSound( To.Everyone, "explosion_short_tail" );
		plunger.Delete();
		FinishDie();
	}

	[ClientRpc]
	public void PlayDeathSound( string sound )
	{
		this.SoundFromScreen( sound );
	}

	private void FinishDie()
	{
		Log.Info( $"{Name} has successfully died." );
		TextChat.AddInfoChatEntry( DeathReason.ToString() );

		LifeState = LifeState.Dead;
		EnableDrawing = false;
		Tags.Remove( "player" );
		Tags.Add( "dead" );

		if ( !DeathReason.FromKillTrigger )
		{
			var gravestone = PrefabLibrary.Spawn<Gadget>( "prefabs/world/gravestone.prefab" );
			gravestone.Owner = Player;
			gravestone.Position = Position;
			Player.Gadgets.Add( gravestone );
		}
	}

	[ConCmd.Admin( "kill" )]
	private static void Kill()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( !player.IsTurn )
			return;

		player.ActiveGrub.TakeDamage( DamageInfo.Generic( float.MaxValue ).WithTag( "admin" ) );
	}
}
