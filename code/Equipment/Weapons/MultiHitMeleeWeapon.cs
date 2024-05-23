﻿using Grubs.Common;
using Grubs.Pawn;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Multi Hit Melee Weapon" ), Category( "Equipment" )]
public class MultiHitMeleeWeapon : Weapon
{
	/**
	 * Meta
	 */
	[Property] public int Strikes { get; set; } = 2;

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


	private TimeSince _timeSinceLastHit = 0;
	private int _currentStrikeCount = 1;
	private int _numberOfSwings = 0;
	private bool _finishAfterCooldown = false;

	protected override void FireImmediate()
	{
		HitEffects();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( _timeSinceLastHit > Cooldown )
			if ( _finishAfterCooldown )
				FireFinished();

		if ( _timeSinceLastHit > 2f )
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
		if ( Equipment.Grub is not { } grub )
			return;

		_currentStrikeCount = 1;
		grub.Animator.Punch( _currentStrikeCount );
	}

	[Broadcast]
	private void HitEffects()
	{
		if ( _timeSinceLastHit < HitCooldown )
			return;

		if ( _numberOfSwings >= Strikes || _finishAfterCooldown )
			return;

		if ( Equipment.Grub is not { } grub )
			return;

		grub.Animator.Punch( _currentStrikeCount - 1 );
		grub.Animator.Fire();

		_timeSinceLastHit = 0f;

		var trs = GetHitObjects();
		var damage = BaseHitDamage;

		if ( _currentStrikeCount == Strikes )
		{
			damage += FinalHitModifier * _currentStrikeCount;
			foreach ( var tr in trs )
			{
				if ( tr.GameObject.Components.TryGet( out Grub hitGrub, FindMode.EverythingInSelfAndAncestors ) )
					HandleGrubHit( hitGrub, damage, (tr.Direction + Vector3.Up) * FinalHitForce, true );

				if ( tr.GameObject.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelfAndAncestors ) )
					HandleBodyHit( body, damage, tr.HitPosition, (tr.Direction + Vector3.Up) * FinalHitForce );
			}

			_currentStrikeCount = 1;
			TimeSinceLastUsed = 0f;
		}
		else
		{
			damage += HitComboModifier * _currentStrikeCount;
			foreach ( var tr in trs )
			{
				if ( tr.GameObject.Components.TryGet( out Grub hitGrub, FindMode.EverythingInSelfAndAncestors ) )
					HandleGrubHit( hitGrub, damage, (tr.Direction + Vector3.Up) * BaseHitForce );

				if ( tr.GameObject.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelfAndAncestors ) )
					HandleBodyHit( body, damage, tr.HitPosition, (tr.Direction + Vector3.Up) * BaseHitDamage );
			}

			_currentStrikeCount += 1;
		}

		_numberOfSwings += 1;

		if ( _numberOfSwings >= Strikes )
		{
			_currentStrikeCount = 1;
			_numberOfSwings = 0;
			_finishAfterCooldown = true;
			return;
		}
	}

	private IEnumerable<SceneTraceResult> GetHitObjects()
	{
		var res = new List<SceneTraceResult>();

		if ( Equipment.Grub is not { } grub )
			return res;

		var ctrl = grub.PlayerController;
		var startPos = ctrl.Transform.Position + Vector3.Up * 24f + HitOffset;
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
			grub.CharacterController.ReleaseFromGround();
		grub.CharacterController.Punch( force );
		grub.Health.TakeDamage( GrubsDamageInfo.FromMelee( damage, Equipment.Grub.GameObject, GameObject ) );
	}

	private void HandleBodyHit( Rigidbody body, float damage, Vector3 position, Vector3 force )
	{
		if ( body.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
			health.TakeDamage( GrubsDamageInfo.FromMelee( damage, Equipment.Grub.GameObject, GameObject, position ) );

		body.ApplyImpulseAt( position, force * body.PhysicsBody.Mass );
	}
}
