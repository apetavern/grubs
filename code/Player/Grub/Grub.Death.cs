namespace Grubs;

public partial class Grub
{
	public DeathReason DeathReason { get; set; }

	public Task DeathTask { get; set; }

	[Net]
	public Gravestone Gravestone { get; set; }

	[Net]
	public bool HasBeenDamaged { get; set; }

	public bool ShouldTakeDamage { get; set; }

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

		TakeDamage( DamageInfo.Generic( Math.Min( totalDamage, Health ) ) );

		ShouldTakeDamage = false;
		HasBeenDamaged = false;
		return dead;
	}

	public override void OnKilled()
	{
		if ( LifeState is LifeState.Dying or LifeState.Dead )
			return;

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

		Log.Info( DeathReason.ToString() );

		ExplosionHelper.Explode( Position, this, 50f );
		plunger.Delete();
		FinishDie();
	}

	private void FinishDie()
	{
		LifeState = LifeState.Dead;
		EnableDrawing = false;

		Gravestone = new Gravestone( this )
		{
			Owner = this,
			Parent = this,
		};
	}
}
