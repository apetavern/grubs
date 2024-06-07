using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;

namespace Grubs;

[Title( "Grubs - Airstrike Plane" ), Category( "Equipment" )]
public sealed class AirstrikePlane : TargetedProjectile
{
	[Property] public float DropRange { get; set; } = 25f;
	[Property] public GameObject DropPrefab { get; set; }
	[Property] public int AmountToDrop { get; set; } = 3;
	[Property] public bool ApplyVelocity { get; set; }

	private bool _fired;
	private SoundHandle _engineSound;
	private bool _fading;

	public override void ShareData()
	{
		base.ShareData();
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 1.1f ).WithX( -Direction.x * GrubsConfig.TerrainLength * 1.05f );
		Transform.Rotation = Rotation.LookAt( Direction );
		_engineSound = Sound.Play( "plane_engine_loop" );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( _engineSound.IsValid )
		{
			_engineSound.Position = Transform.Position;
		}

		if ( MathF.Abs( Transform.Position.x - ProjectileTarget.x ) < DropRange * 1.25f && !_fired )
		{
			Model.Set( "open", true );
		}

		if ( MathF.Abs( Transform.Position.x - ProjectileTarget.x ) < DropRange && !_fired )
		{
			_fired = true;
			DropBombs();
		}

		if ( MathF.Abs( Transform.Position.x ) > GrubsConfig.TerrainLength * 1.1f && !_fading )
		{
			_fading = true;
			FadeOut();
		}
		else
		{
			Transform.Position += Direction * ProjectileSpeed * Time.Delta;
		}

		Transform.Rotation = Transform.Rotation.Angles().WithRoll( MathF.Sin( Time.Now * 2f ) * 5f );
	}

	async void FadeOut()
	{
		while ( Model.Tint.a > 0 )
		{
			Model.Tint = Model.Tint.WithAlpha( Model.Tint.a - Time.Delta );
			if ( _engineSound.IsValid && _engineSound.Volume > 0 )
				_engineSound.Volume -= Time.Delta;
			await Task.Frame();
		}

		GameObject.Destroy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( _engineSound.IsValid )
			_engineSound.Stop();

	}

	public async void DropBombs()
	{
		Sound.Play( "plane_bay_door_open" );
		for ( int i = 0; i < AmountToDrop; i++ )
		{
			var bomb = DropPrefab.Clone();
			bomb.NetworkSpawn();

			if ( i == AmountToDrop / 2 )
				GrubFollowCamera.Local.SetTarget( bomb, 5f );

			bomb.Transform.Position = Model.GetAttachment( "droppoint" ).Value.Position;
			bomb.Transform.Rotation = Transform.Rotation * Rotation.FromPitch( 25f );
			if ( ApplyVelocity && bomb.Components.TryGet( out Rigidbody body ) )
			{
				body.Velocity = Direction * ProjectileSpeed;
			}

			if ( bomb.Components.TryGet( out Projectile projectile ) )
			{
				projectile.SourceId = SourceId;
			}
			await Task.DelaySeconds( 0.25f );
		}

		Model.Set( "open", false );
	}
}
