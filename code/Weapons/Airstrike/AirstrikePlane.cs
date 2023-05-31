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

	public bool LeftToRight { get; set; }
	public float TargetX { get; set; }

	private float _speed = 9;

	[GameEvent.Tick]
	void OnTick()
	{
		if ( Game.IsServer )
		{
			var dir = LeftToRight ? Vector3.Backward : Vector3.Forward;
			Position += dir * _speed;
			if ( Position.x.AlmostEqual( TargetX, LeftToRight ? 3 : -3 ) )
			{
				DropPayload();
				DeleteAsync( 10 );
			}
		}
	}

	private async void DropPayload()
	{
		for ( int i = 0; i < ProjectileCount; i++ )
		{
			if ( !PrefabLibrary.TrySpawn<Entity>( Projectile.ResourcePath, out var bomb ) )
				continue;

			bomb.Owner = Owner;
			bomb.Position = Position + DropOffset;

			foreach ( var comp in bomb.Components.GetAll<GadgetComponent>() )
				comp.OnUse( null, 0 );

			bomb.DeleteAsync( 3 );
			await GameTask.DelaySeconds( DropRateSeconds );
		}
	}
}
