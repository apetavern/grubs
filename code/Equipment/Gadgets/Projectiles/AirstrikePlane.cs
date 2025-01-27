using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Airstrike Plane" ), Category( "Equipment" )]
public sealed class AirstrikePlane : TargetedProjectile
{
	[Property] public GameObject DropPrefab { get; set; }
	[Property] public bool ApplyVelocity { get; set; }
	
	[Property] private float DropRange { get; set; } = 25f;
	[Property] private int AmountToDrop { get; set; } = 3;

	private SoundHandle EngineSoundHandle { get; set; }
	private bool Fired { get; set; }
	private bool IsFadingOut { get; set; }
	private bool HasClosedHatch { get; set; }
	
	private TimeSince TimeSinceBombDropped { get; set; }
	private int BombsDropped { get; set; } = 0;

	protected override void OnStart()
	{
		EngineSoundHandle = Sound.Play( "plane_engine_loop" );
	}

	public override void ShareData()
	{
		base.ShareData();
		
		WorldPosition = ProjectileTarget
			.WithX( -Direction.x * GrubsTerrain.Instance.WorldTextureLength * 1.05f )
			.WithZ( GrubsTerrain.Instance.WorldTextureHeight * 1.1f );
		WorldRotation = Rotation.LookAt( Direction );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateEngineSound();
		UpdateHatchAnimation();
		
		if ( !IsProxy )
			UpdateBombDropping();


		if ( MathF.Abs( WorldPosition.x ) > GrubsTerrain.Instance.WorldTextureLength * 1.1f && !IsFadingOut )
		{
			IsFadingOut = true;
			_ = FadeOut();
		}

		WorldPosition += Direction * ProjectileSpeed * Time.Delta;

		const float speed = 2f;
		const float swivel = 5f;
		var roll = MathF.Sin( Time.Now * speed ) * swivel;
		WorldRotation = WorldRotation.Angles().WithRoll( roll );
	}

	private void UpdateEngineSound()
	{
		if ( !EngineSoundHandle.IsValid() )
			return;
		
		EngineSoundHandle.Position = WorldPosition;
	}

	private void UpdateHatchAnimation()
	{
		if ( !Model.IsValid() || Fired )
			return;

		var dropBreadth = DropRange * 1.25f;

		if ( MathF.Abs( WorldPosition.x - ProjectileTarget.x ) >= dropBreadth )
			return;
	
		Model.Set( "open", true );
	}

	private async Task FadeOut()
	{
		while ( Model.Tint.a > 0 )
		{
			Model.Tint = Model.Tint.WithAlpha( Model.Tint.a - Time.Delta );
			if ( EngineSoundHandle.IsValid() && EngineSoundHandle.Volume > 0 )
				EngineSoundHandle.Volume -= Time.Delta;
			await Task.Frame();
		}

		await Task.MainThread();
		GameObject.Destroy();
	}

	private void DropBomb()
	{
		if ( !DropPrefab.IsValid() )
			return;
		
		var bomb = DropPrefab.Clone();
		bomb.NetworkSpawn();
		
		GrubFollowCamera.Local?.QueueTarget( bomb, 5f );

		var dropPoint = Model.GetAttachment( "droppoint" ).GetValueOrDefault();
		bomb.WorldPosition = dropPoint.Position;
		bomb.WorldRotation = dropPoint.Rotation;

		var body = bomb.GetComponent<Rigidbody>();
		if ( body.IsValid() && ApplyVelocity )
			body.Velocity = Direction * ProjectileSpeed;
		
		var projectile = bomb.GetComponent<Projectile>();
		if ( projectile.IsValid() )
			projectile.SourceId = SourceId;
	}

	[Rpc.Broadcast]
	private void HatchOpenEffects()
	{
		Sound.Play( "plane_bay_door_open" );
		Tags.Add( "notarget" );
	}

	[Rpc.Broadcast]
	private void HatchCloseEffects()
	{
		if ( !Model.IsValid() )
			return;
		
		HasClosedHatch = true;
		Model.Set( "open", false );
	}

	private void UpdateBombDropping()
	{
		const float timeBetweenBombDrops = 0.1f;

		if ( MathF.Abs( WorldPosition.x - ProjectileTarget.x ) < DropRange && !Fired )
		{
			HatchOpenEffects();
			Fired = true;
		}

		if ( !Fired )
			return;

		if ( TimeSinceBombDropped > timeBetweenBombDrops && BombsDropped < AmountToDrop )
		{
			BombsDropped += 1;

			Log.Info( $"Dropping a bomb ({BombsDropped}): {TimeSinceBombDropped}" );

			DropBomb();
			TimeSinceBombDropped = 0f;
			return;
		}

		if ( HasClosedHatch ) 
			return;
		
		HasClosedHatch = true;
		HatchCloseEffects();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if ( EngineSoundHandle.IsValid() )
			EngineSoundHandle.Stop();
	}
}
