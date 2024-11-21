using Grubs.Common;
using Grubs.Pawn;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Multi Hit Melee Weapon" ), Category( "Equipment" )]
public class MultiHitMeleeWeapon : Weapon
{
	[Property] public float HitCooldown { get; set; } = 0.25f;
	[Property] public float HitDistance { get; set; } = 25f;
	[Property] public Vector3 HitOffset { get; set; }

	/*
	 * Damage
	 */
	[Property] public float BaseHitDamage { get; set; } = 10f;
	[Property] public float HitComboModifier { get; set; } = 1.2f;
	[Property] public float FinalHitModifier { get; set; } = 2.5f;

	/**
	 * Force
	 */
	[Property] public Vector3 BaseHitForce { get; set; }
	[Property] public Vector3 FinalHitForce { get; set; }

	[Property] private SoundEvent ImpactSound { get; set; }

	private TimeSince _timeSinceLastHit = 0;
	private int _currentStrikeCount = 1;
	private bool _finishAfterCooldown = false;

	protected override void FireImmediate()
	{
		HitEffects();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		if ( _timeSinceLastHit > Cooldown )
			if ( _finishAfterCooldown )
				FireFinished();

		if ( _timeSinceLastHit > 3f && _currentStrikeCount != 1 )
			ResetCombo();
	}

	protected override void FireFinished()
	{
		base.FireFinished();

		_finishAfterCooldown = false;
	}

	[Broadcast]
	private void ResetCombo()
	{
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		var anim = Equipment.Grub.Animator;
		if ( !anim.IsValid() )
			return;

		_currentStrikeCount = 1;
		anim.Punch( _currentStrikeCount );
	}

	[Broadcast]
	private void HitEffects()
	{
		if ( _timeSinceLastHit < HitCooldown )
			return;

		if ( TimesUsed >= MaxUses || _finishAfterCooldown )
			return;

		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		var anim = Equipment.Grub.Animator;
		if ( !anim.IsValid() )
			return;

		anim.Punch( _currentStrikeCount - 1 );
		anim.Fire();

		_timeSinceLastHit = 0f;

		var trs = GetHitObjects();
		var damage = BaseHitDamage;
		var playedSound = false;

		if ( _currentStrikeCount == MaxUses )
		{
			damage += FinalHitModifier * _currentStrikeCount;
			foreach ( var tr in trs )
			{
				// Same deal as MeleeWeapon.
				var direction = tr.Direction;
				if ( tr.Direction == Vector3.Zero )
				{
					direction = (Equipment.Grub.WorldPosition - tr.HitPosition).ClampLength( 1f );
					direction = direction.WithX( direction.x * 2 ).WithZ( direction.z / 2 );
				}

				if ( !playedSound )
				{
					Sound.Play( ImpactSound );
					playedSound = true;
				}

				if ( tr.GameObject.Components.TryGet( out Grub hitGrub, FindMode.EverythingInSelfAndAncestors ) )
					HandleGrubHit( hitGrub, damage, (direction + Vector3.Up) * FinalHitForce, true );

				if ( tr.GameObject.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelfAndAncestors ) )
					HandleBodyHit( body, damage, tr.HitPosition, (direction + Vector3.Up) * FinalHitForce );
			}

			_currentStrikeCount = 1;
			TimeSinceLastUsed = 0f;
		}
		else
		{
			damage += HitComboModifier * _currentStrikeCount;
			foreach ( var tr in trs )
			{
				if ( !playedSound )
				{
					Sound.Play( ImpactSound );
					playedSound = true;
				}

				if ( tr.GameObject.Components.TryGet( out Grub hitGrub, FindMode.EverythingInSelfAndAncestors ) )
					HandleGrubHit( hitGrub, damage, (tr.Direction + Vector3.Up) * BaseHitForce );

				if ( tr.GameObject.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelfAndAncestors ) )
					HandleBodyHit( body, damage, tr.HitPosition, (tr.Direction + Vector3.Up) * BaseHitDamage );
			}

			_currentStrikeCount += 1;
		}

		// Delay for last attack so we don't stomp animation.
		if ( TimesUsed == MaxUses - 1 )
			_finishAfterCooldown = true;
		else
			FireFinished();
	}

	private IEnumerable<SceneTraceResult> GetHitObjects()
	{
		var res = new List<SceneTraceResult>();

		if ( Equipment.Grub is not { } grub )
			return res;

		var ctrl = grub.PlayerController;
		var startPos = ctrl.WorldPosition + Vector3.Up * 24f + HitOffset;
		var trs = Scene.Trace.Ray(
				startPos, startPos + ctrl.EyeRotation.Forward * ctrl.Facing * HitDistance )
			.Size( 12f )
			.IgnoreGameObjectHierarchy( grub.GameObject )
			.WithoutTags( "dead" )
			.RunAll();

		foreach ( var tr in trs )
		{
			if ( tr.GameObject is null )
				continue;

			res.Add( tr );
		}

		return res;
	}

	private void HandleGrubHit( Grub grub, float damage, Vector3 force, bool releaseFromGround = false )
	{
		if ( releaseFromGround )
		{
			grub.WorldPosition += force.ClampLength( 1f ) * 5f;
			grub.CharacterController.ReleaseFromGround();

			GrubFollowCamera.Local.SetTarget( grub.GameObject, 1 );
		}

		grub.CharacterController.Punch( force );
		grub.Health.TakeDamage( GrubsDamageInfo.FromMelee( damage, Equipment.Grub.Id, Equipment.Grub.Name ) );
	}

	private void HandleBodyHit( Rigidbody body, float damage, Vector3 position, Vector3 force )
	{
		if ( body.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
			health.TakeDamage( GrubsDamageInfo.FromMelee( damage, Equipment.Grub.Id, Equipment.Grub.Name, position ) );

		body.ApplyImpulseAt( position, force * body.PhysicsBody.Mass );
	}
}
