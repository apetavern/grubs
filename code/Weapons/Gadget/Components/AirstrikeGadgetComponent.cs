namespace Grubs;

[Prefab]
public partial class AirstrikeGadgetComponent : GadgetComponent
{
	[Prefab]
	public Prefab Projectile { get; set; }

	[Prefab]
	public float DropRateSeconds { get; set; } = 0.2f;

	[Prefab]
	public int ProjectileCount { get; set; } = 1;

	[Net]
	public bool RightToLeft { get; set; }

	public bool HasReachedTarget { get; private set; }
	public Vector3 TargetPosition { get; set; }

	private float _speed = 20;

	public override void Simulate( IClient client )
	{
		Gadget.Position += RightToLeft ? Vector3.Backward * _speed : Vector3.Forward * _speed;
		var currentDistanceFromTarget = Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( TargetPosition.WithY( 0 ).WithZ( 0 ) );

		// Start the animation a bit early to give it enough time.
		if ( currentDistanceFromTarget <= 300 )
			Gadget.SetAnimParameter( "open", true );

		if ( currentDistanceFromTarget <= 200 && !HasReachedTarget )
		{
			HasReachedTarget = true;

			if ( Game.IsServer )
			{
				Gadget.ShouldCameraFollow = false;
				DropPayload();
			}
		}

		if ( Game.IsServer && (HasReachedTarget && Gadget.Position.WithY( 0 ).WithZ( 0 ).Distance( GrubsGame.Instance.Terrain.Position.WithY( 0 ).WithZ( 0 ) ) >= GrubsConfig.TerrainLength * 3) )
			Gadget.Delete();
	}

	private async void DropPayload()
	{
		var dropAttachment = Gadget.GetAttachment( "droppoint", false );
		for ( int i = 0; i < ProjectileCount; i++ )
		{
			await GameTask.DelaySeconds( DropRateSeconds );

			if ( !PrefabLibrary.TrySpawn<Gadget>( Projectile.ResourcePath, out var bomb ) )
				continue;

			Gadget.Grub.AssignGadget( bomb );
			bomb.Position = dropAttachment.HasValue ? Gadget.Position + dropAttachment.Value.Position : Gadget.Position;

			if ( i == 0 )
				bomb.ShouldCameraFollow = true;

			if ( bomb.Components.TryGet<ArcPhysicsGadgetComponent>( out var arcPhysics ) )
				arcPhysics.OnUse( null, 0 );
		}
	}
}
