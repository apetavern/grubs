namespace Grubs;

[Prefab]
public partial class AirstrikePlane : AnimatedEntity
{
	/// <summary>
	/// What are we dropping?
	/// </summary>
	[Prefab]
	public Prefab Projectile { get; set; }

	[Prefab]
	public float DropRateSeconds { get; set; } = 0.2f;

	/// <summary>
	/// How many are we dropping?
	/// </summary>
	[Prefab]
	public int ProjectileCount { get; set; } = 1;

	[Prefab]
	public Vector3 DropOffset { get; set; }

	[Net]
	public bool RightToLeft { get; set; }

	public bool HasReachedTarget { get; private set; }
	public Vector3 TargetPosition { get; set; }

	private float _speed = 18;

	public override void Spawn()
	{
		GamemodeSystem.Instance.CameraTarget = this;
	}

	[GameEvent.Tick.Server]
	void OnTick()
	{
		var dir = RightToLeft ? Vector3.Backward : Vector3.Forward;
		Position += dir * _speed;

		const float xLookAhead = 200;
		var targetX = TargetPosition.x;

		bool withinTargetPosition = (RightToLeft && Position.x <= targetX + xLookAhead) || (!RightToLeft && Position.x >= targetX - xLookAhead);
		if ( withinTargetPosition && !HasReachedTarget )
		{
			HasReachedTarget = true;
			DropPayload();
			DeleteAsync( 10 );
		}
	}

	private async void DropPayload()
	{
		for ( int i = 0; i < ProjectileCount; i++ )
		{
			if ( !PrefabLibrary.TrySpawn<Entity>( Projectile.ResourcePath, out var bomb ) )
				continue;

			if ( ProjectileCount == 1 )
				GamemodeSystem.Instance.CameraTarget = bomb;
			else if ( i == ProjectileCount / 2 )
				GamemodeSystem.Instance.CameraTarget = bomb;

			var drop = GetAttachment( "droppoint", true );
			var dropPosition = drop.HasValue ? drop.Value.Position : Position + Vector3.Down * 32;

			bomb.Owner = Owner;
			bomb.Position = dropPosition;

			foreach ( var comp in bomb.Components.GetAll<GadgetComponent>() )
				comp.OnUse( null, 0 );

			bomb.DeleteAsync( 3 );
			await GameTask.DelaySeconds( DropRateSeconds );
		}
	}
}
