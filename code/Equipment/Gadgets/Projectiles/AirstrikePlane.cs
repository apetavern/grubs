using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Pawn;
using Grubs.Terrain;

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
		WorldPosition = ProjectileTarget.WithZ( GrubsTerrain.Instance.WorldTextureHeight * 1.1f ).WithX( -Direction.x * GrubsTerrain.Instance.WorldTextureLength * 1.05f );
		WorldRotation = Rotation.LookAt( Direction );
		_engineSound = Sound.Play( "plane_engine_loop" );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( _engineSound.IsValid() )
		{
			_engineSound.Position = WorldPosition;
		}

		if ( MathF.Abs( WorldPosition.x - ProjectileTarget.x ) < DropRange * 1.25f && !_fired )
		{
			Model.Set( "open", true );
		}

		if ( MathF.Abs( WorldPosition.x - ProjectileTarget.x ) < DropRange && !_fired )
		{
			_fired = true;
			
			if ( !IsProxy )
				DropBombs();
		}

		if ( MathF.Abs( WorldPosition.x ) > GrubsTerrain.Instance.WorldTextureLength * 1.1f && !_fading )
		{
			_fading = true;
			FadeOut();
		}
		else
		{
			WorldPosition += Direction * ProjectileSpeed * Time.Delta;
		}

		WorldRotation = WorldRotation.Angles().WithRoll( MathF.Sin( Time.Now * 2f ) * 5f );
	}

	async void FadeOut()
	{
		while ( Model.Tint.a > 0 )
		{
			Model.Tint = Model.Tint.WithAlpha( Model.Tint.a - Time.Delta );
			if ( _engineSound.IsValid() && _engineSound.Volume > 0 )
				_engineSound.Volume -= Time.Delta;
			await Task.Frame();
		}

		GameObject.Destroy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( _engineSound.IsValid() )
			_engineSound.Stop();

	}

	public async void DropBombs()
	{`
		Sound.Play( "plane_bay_door_open" );
		for ( int i = 0; i < AmountToDrop; i++ )
		{
			var bomb = DropPrefab.Clone();
			bomb.NetworkSpawn();

			if ( i == AmountToDrop / 2 )
				GrubFollowCamera.Local.QueueTarget( bomb, 5f );

			bomb.WorldPosition = Model.GetAttachment( "droppoint" ).Value.Position;
			bomb.WorldRotation = WorldRotation * Rotation.FromPitch( 25f );
			if ( ApplyVelocity && bomb.Components.TryGet( out Rigidbody body ) )
			{
				body.Velocity = Direction * ProjectileSpeed;
			}

			if ( bomb.Components.TryGet( out Projectile projectile ) )
			{
				projectile.SourceId = SourceId;
			}
			await Task.DelaySeconds( 0.1f );
		}

		Model.Set( "open", false );
	}
}
