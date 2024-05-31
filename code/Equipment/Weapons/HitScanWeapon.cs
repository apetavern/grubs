using Grubs.Common;
using Grubs.Helpers;
using Grubs.Pawn;
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
	[Property, Category( "Effects" )] public SoundEvent TraceSound { get; set; }
	[Property, Category( "Effects" )] public ParticleSystem TraceParticles { get; set; }
	[Property, Category( "Effects" )] public ParticleSystem MuzzleParticles { get; set; }

	private int _tracesFired = 0;
	private TimeSince _timeSinceLastTrace = 0;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsFiring && _tracesFired < TraceCount )
			FireImmediate();
	}

	protected override void FireImmediate()
	{
		if ( Equipment.Grub is not { } grub )
			return;

		if ( _timeSinceLastTrace < TraceDelay )
			return;

		var startPos = GetStartPosition();
		var pc = grub.PlayerController;
		var endPos = startPos + pc.Facing * pc.EyeRotation.Forward * TraceDistance + Vector3.Random * TraceSpread;

		_tracesFired++;
		_timeSinceLastTrace = 0;
		FireEffects( startPos, endPos );

		if ( _tracesFired >= TraceCount )
			FireFinished();
	}

	protected override void FireFinished()
	{
		base.FireFinished();

		_tracesFired = 0;
	}

	[Broadcast]
	private void FireEffects( Vector3 startPos, Vector3 endPos )
	{
		if ( Equipment.Grub is not { } grub )
			return;

		grub.Animator.Fire();
		Sound.Play( TraceSound, startPos );

		var transform = new Transform( startPos, grub.PlayerController.EyeRotation );
		if ( MuzzleParticles is not null )
		{
			var muzzle = Equipment.Model.GetAttachment( "muzzle" );
			ParticleHelper.Instance.PlayInstantaneous( MuzzleParticles, muzzle ?? transform );
		}

		var tr = Scene.Trace.Ray( startPos, endPos )
			.WithAnyTags( "solid", "player", "pickup" )
			.WithoutTags( "dead" )
			.IgnoreGameObjectHierarchy( Equipment.Grub.GameObject );

		if ( PenetrateWorld )
		{
			tr = tr.WithoutTags( "solid" );
			using ( Rpc.FilterInclude( c => c.IsHost ) )
			{
				GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ), ExplosionRadius, 1 );
				GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
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
		if ( TraceParticles is not null )
		{
			var transform = new Transform( tr.StartPosition );
			var traceParticles = ParticleHelper.Instance.PlayInstantaneous( TraceParticles, transform );
			traceParticles.SetControlPoint( 1, tr.EndPosition );
		}

		if ( !tr.Hit )
			return false;

		if ( tr.GameObject.Components.TryGet<Grub>( out var grub, FindMode.Enabled | FindMode.InAncestors ) )
		{
			HitGrub( grub, -tr.Normal, tr.HitPosition );
			return false;
		}
		else if ( tr.GameObject.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
		{
			health.TakeDamage( GrubsDamageInfo.FromHitscan( Damage, Equipment.Grub, GameObject, tr.HitPosition ) );
			return false;
		}

		ExplosionHelper.Instance.Explode( this, tr.EndPosition, ExplosionRadius, ExplosionDamage );
		return true;
	}

	private void HitGrub( Grub grub, Vector3 direction, Vector3 position )
	{
		grub.CharacterController.Punch( direction * HitForce );
		grub.Health.TakeDamage( GrubsDamageInfo.FromHitscan( Damage, Equipment.Grub, GameObject, position ) );
	}
}
