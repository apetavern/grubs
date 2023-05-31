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
	public float TotalDamageTaken { get; set; }

	private Queue<DamageInfo> DamageInfoQueue { get; set; } = new();

	public override void TakeDamage( DamageInfo info )
	{
		if ( !Game.IsServer )
			return;

		SetAnimParameter( "hit", true );
		SetAnimParameter( "hit_direction", info.Force );

		if ( !ShouldTakeDamage )
		{
			if ( IsTurn && GamemodeSystem.Instance is FreeForAll )
				GamemodeSystem.Instance.UseTurn( false );

			// Only add a grub if they are not already in the queue.
			// The damage info queue will handle any stacking damage.
			if ( !GamemodeSystem.Instance.DamageQueue.Contains( this ) )
				GamemodeSystem.Instance.DamageQueue.Enqueue( this );

			DamageInfoQueue.Enqueue( info );
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

	public float ApplyDamage()
	{
		ShouldTakeDamage = true;

		var totalDamage = 0f;
		var damageInfo = new List<DamageInfo>();
		while ( DamageInfoQueue.TryDequeue( out var dmgInfo ) )
		{
			damageInfo.Add( dmgInfo );
			totalDamage += dmgInfo.Damage;
		}

		if ( totalDamage >= Health )
			DeathReason = DeathReason.FindReason( this, damageInfo );

		TakeDamage( DamageInfo.Generic( Math.Min( totalDamage, Health ) ) );

		TotalDamageTaken = totalDamage;
		ShouldTakeDamage = false;
		HasBeenDamaged = false;

		return totalDamage;
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

		// This grub joined in late, don't explode them, just kill them!
		if ( Components.Get<LateJoinComponent>() is null )
		{
			var plunger = new ModelEntity( "models/tools/dynamiteplunger/dynamiteplunger.vmdl" )
			{
				Position = Facing == -1 ? Position - new Vector3( 30, 0, 0 ) : Position
			};

			await GameTask.Delay( 1025 );

			ExplosionHelper.Explode( Position, this, 50f );
			PlayDeathSound( To.Everyone, "explosion_short_tail" );
			plunger.Delete();
		}

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
		UI.TextChat.AddInfoChatEntry( DeathReason.ToString() );

		LifeState = LifeState.Dead;
		Tags.Remove( Tag.Player );
		Tags.Add( Tag.Dead );

		EnableDrawing = false;

		// Disable drawing of any clothes, for whatever reason EnableDrawing doesn't handle it.
		var clothes = Children.OfType<AnimatedEntity>().Where( child => child.Tags.Has( Tag.Clothing ) );
		foreach ( var clothing in clothes )
			clothing.EnableDrawing = false;

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

		player.ActiveGrub.TakeDamage( DamageInfo.Generic( float.MaxValue ).WithTag( Tag.Admin ) );
	}
}
