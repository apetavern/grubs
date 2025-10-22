using Grubs.Common;
using Grubs.Helpers;
using Grubs.Pawn;
using Grubs.Systems.Particles;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Hit Scan Weapon Component" ), Category( "Equipment" )]
public class HitScanWeapon : Weapon
{
	[Property] public float Damage { get; set; } = 20f;
	[Property] public Vector3 HitForce { get; set; }
	[Property] public bool PenetrateTargets { get; set; } = false;
	[Property] public bool PenetrateWorld { get; set; } = false;
	[Property, Category( "Trace" )] public float TraceCount { get; set; } = 1f;
	[Property, Category( "Trace" )] public float TraceDistance { get; set; } = 0f;
	[Property, Category( "Trace" )] public float TraceSpread { get; set; } = 1f;
	[Property, Category( "Trace" )] public float TraceDelay { get; set; } = 0f;
	[Property, Category( "Explosion" )] public float ExplosionRadius { get; set; } = 0f;
	[Property, Category( "Explosion" )] public float ExplosionDamage { get; set; } = 0f;

	private int _tracesFired = 0;
	private TimeSince _timeSinceLastTrace = 0;

	private TracerParticles _tracerParticles { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsFiring && _tracesFired < TraceCount )
			FireImmediate();
	}

	protected override void FireImmediate()
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		if ( _timeSinceLastTrace < TraceDelay )
			return;

		var grub = Equipment.Grub;

		var startPos = GetStartPosition();
		var pc = grub.PlayerController;
		if ( !pc.IsValid() )
			return;
		var endPos = startPos + pc.Facing * pc.EyeRotation.Forward * TraceDistance + Vector3.Random.WithY( 0 ) * TraceSpread;

		_tracesFired++;
		_timeSinceLastTrace = 0;
		FireEffects( startPos, endPos, _tracesFired );

		if ( _tracesFired >= TraceCount )
			FireFinished();
	}

	protected override void FireFinished()
	{
		base.FireFinished();

		_tracesFired = 0;
	}

	[Rpc.Broadcast]
	private void FireEffects( Vector3 startPos, Vector3 endPos, int traceCount )
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		var grub = Equipment.Grub;
		grub.Animator?.Fire();

		if ( TraceDelay > 0 || TraceDelay == 0 && traceCount == 1 )
			Sound.Play( UseSound, startPos );

		var transform = new Transform( startPos, grub.PlayerController.EyeRotation );
		var muzzle = Equipment.Model.GetAttachment( "muzzle" );

		var pitch = muzzle?.Rotation.Pitch() ?? transform.Rotation.Pitch();
		Log.Info( pitch );
		var facing = grub.PlayerController.Facing;
		if ( facing < 0 )
		{
			pitch -= 180f;
		}

		MuzzleParticles.Spawn()
			.SetWorldPosition( muzzle?.Position ?? transform.Position )
			.SetPitch( pitch * facing );

		var tr = Scene.Trace.Ray( startPos, endPos )
			.WithAnyTags( "solid", "player", "pickup" )
			.WithoutTags( "dead" )
			.IgnoreGameObjectHierarchy( Equipment.Grub.GameObject );

		if ( traceCount > 0 )
		{
			_tracerParticles = TracerParticles.Spawn( PenetrateWorld ).SetWorldPosition( startPos ).SetEndPoint( endPos );
		}

		if ( PenetrateWorld )
		{
			using ( Rpc.FilterInclude( c => c.IsHost ) )
			{
				// if ( TraceParticles is not null )
				// {
				// 	var traceParticles = ParticleHelper.Instance?.PlayInstantaneous( TraceParticles, new Transform( startPos ) );
				// 	traceParticles?.SetControlPoint( 1, new Transform( endPos ) );
				// }

				GrubsTerrain.Instance?.SubtractLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ), ExplosionRadius, 1 );
				GrubsTerrain.Instance?.ScorchLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ),
					ExplosionRadius + 8f );
			}
		}

		if ( PenetrateTargets )
		{
			foreach ( var result in tr.RunAll() )
				if ( HandleTraceHit( result ) )
					return;
		}
		else
		{
			HandleTraceHit( tr.Run() );
		}
	}

	private bool HandleTraceHit( SceneTraceResult tr )
	{
		// if ( TraceParticles is not null && !PenetrateWorld )
		// {
		// 	var transform = new Transform( tr.StartPosition );
		// 	var traceParticles = ParticleHelper.Instance.PlayInstantaneous( TraceParticles, transform );
		// 	traceParticles.SetControlPoint( 1, tr.EndPosition );
		// }

		if ( _tracerParticles.IsValid() && !PenetrateWorld )
		{
			_tracerParticles.SetEndPoint( tr.Hit ? tr.HitPosition : tr.EndPosition );
		}

		if ( !tr.Hit )
			return false;

		if ( !tr.GameObject.IsValid() )
			return false;

		if ( tr.GameObject.Components.TryGet<Grub>( out var grub, FindMode.Enabled | FindMode.InAncestors ) )
		{
			if ( !grub.IsValid() )
				return false;
			HitGrub( grub, -tr.Normal, tr.HitPosition );
			return false;
		}

		if ( tr.GameObject.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
		{
			if ( !health.IsValid() )
				return false;
			health.TakeDamage( GrubsDamageInfo.FromHitscan( Damage, Equipment.Grub.Id, Equipment.Grub.Name, tr.HitPosition ) );
			return false;
		}

		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return false;

		if ( !ExplosionHelper.Instance.IsValid() )
			return false;

		ExplosionHelper.Instance.Explode( this, tr.EndPosition, ExplosionRadius, ExplosionDamage, Equipment.Grub.Id, Equipment.Grub.Name, HitForce.x );
		ExplosionParticles.Spawn().SetWorldPosition( tr.EndPosition ).SetScale( ExplosionRadius * 2f );
		return true;
	}

	private void HitGrub( Grub grub, Vector3 direction, Vector3 position )
	{
		if ( !grub.IsValid() || !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		grub.CharacterController?.Punch( direction * HitForce );
		grub.Health?.TakeDamage( GrubsDamageInfo.FromHitscan( Damage, Equipment.Grub.Id, Equipment.Grub.Name, position ) );

		GrubFollowCamera.Local?.QueueTarget( grub.GameObject, 1 );
	}
}
