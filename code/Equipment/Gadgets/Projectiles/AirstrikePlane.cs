using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Grubs.Terrain;
using static Grubs.Equipment.Weapons.Weapon;

namespace Grubs;

[Title( "Grubs - Airstrike Plane" ), Category( "Equipment" )]
public sealed class AirstrikePlane : TargetedProjectile
{
	[Property] public float DropRange { get; set; } = 25f;
	[Property] public GameObject DropPrefab { get; set; }
	[Property] public int AmountToDrop { get; set; } = 3;

	private bool _fired;

	public override void ShareData()
	{
		base.ShareData();
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 1.1f ).WithX( -Direction.x * GrubsConfig.TerrainLength * 1.05f );
		Transform.Rotation = Rotation.LookAt( Direction );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();


		if ( MathF.Abs( Transform.Position.x - ProjectileTarget.x ) < DropRange * 1.25f && !_fired )
		{
			Model.Set( "open", true );
		}

		if ( MathF.Abs( Transform.Position.x - ProjectileTarget.x ) < DropRange && !_fired )
		{
			_fired = true;
			DropBombs();
		}

		if ( MathF.Abs( Transform.Position.x ) > GrubsConfig.TerrainLength * 1.1f )
		{
			GameObject.Destroy();
		}
		else
		{
			Transform.Position += Direction * ProjectileSpeed * Time.Delta;
		}

		Transform.Rotation = Transform.Rotation.Angles().WithRoll( MathF.Sin( Time.Now * 2f ) * 5f );
	}

	public async void DropBombs()
	{
		for ( int i = 0; i < AmountToDrop; i++ )
		{
			var bomb = DropPrefab.Clone();
			bomb.NetworkSpawn();

			if ( i == AmountToDrop / 2 )
				GrubFollowCamera.Local.SetTarget( bomb, 5f );

			bomb.Transform.Position = Model.GetAttachment( "droppoint" ).Value.Position;
			bomb.Transform.Rotation = Transform.Rotation * Rotation.FromPitch( 25f );
			if ( bomb.Components.TryGet( out Rigidbody body ) )
			{
				body.Velocity = Direction * ProjectileSpeed;
			}
			await Task.DelaySeconds( 0.25f );
		}
		Model.Set( "open", false );
	}
}
