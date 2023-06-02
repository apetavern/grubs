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

	private float _speed = 18;

	public override void Simulate( IClient client )
	{
		var dir = RightToLeft ? Vector3.Backward : Vector3.Forward;
		Gadget.Position += dir * _speed;

		if ( !Game.IsServer )
			return;

		const float xLookAhead = 200;
		var targetX = TargetPosition.x;

		bool withinTargetPosition = (RightToLeft && Gadget.Position.x <= targetX + xLookAhead) || (!RightToLeft && Gadget.Position.x >= targetX - xLookAhead);
		if ( withinTargetPosition && !HasReachedTarget )
		{
			HasReachedTarget = true;
			Gadget.ShouldCameraFollow = false;
			DropPayload();
			Gadget.DeleteAsync( 2 );
		}
	}

	private async void DropPayload()
	{
		var drop = Gadget.GetAttachment( "droppoint", true );
		var dropPosition = drop.HasValue ? drop.Value.Position : Gadget.Position;

		for ( int i = 0; i < ProjectileCount; i++ )
		{
			if ( !PrefabLibrary.TrySpawn<Gadget>( Projectile.ResourcePath, out var bomb ) )
				continue;

			bomb.Owner = Gadget.Owner;
			bomb.Position = dropPosition;
			Player.Gadgets.Add( bomb );

			await GameTask.DelaySeconds( DropRateSeconds );
		}
	}
}
