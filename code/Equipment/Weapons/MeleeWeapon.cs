using Grubs.Common;
using Grubs.Pawn;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Melee Weapon" ), Category( "Equipment" )]
public class MeleeWeapon : Weapon
{
	[Property] public float Damage { get; set; } = 25f;
	[Property] public float HitForce { get; set; } = 512f;
	[Property] public Vector3 HitSize { get; set; }
	[Property] public Vector3 HitOffset { get; set; }
	[Property] public float HitDelay { get; set; }
	[Property, ResourceType( "sound" )] public required string ImpactSound { get; set; }

	protected override void FireImmediate()
	{
		if ( Equipment.Grub is not { } grub )
			return;

		var pc = grub.PlayerController;

		var ray = new Ray( grub.Transform.Position + Vector3.Up * 24f + HitOffset, pc.Facing * pc.EyeRotation.Forward );
		HitEffects( ray );

		DelayFireFinished();
	}

	async void DelayFireFinished()
	{
		await Task.DelaySeconds( 0.1f );
		FireFinished();
	}

	[Broadcast]
	public void HitEffects( Ray ray )
	{
		if ( Equipment.Grub is not { } grub )
			return;

		grub.Animator.Fire();

		Sound.Play( UseSound, GetStartPosition() );

		var trs = Scene.Trace.Ray( ray, HitSize.x )
			.Size( 12f )
			.IgnoreGameObjectHierarchy( grub.GameObject )
			.WithoutTags( "dead" )
			.RunAll();

		if ( trs.Any( tr => tr.GameObject is not null ) )
			Sound.Play( ImpactSound, trs.First().HitPosition );

		foreach ( var tr in trs )
		{
			if ( tr.GameObject is null )
				continue;

			if ( tr.GameObject.Components.TryGet( out Grub hitGrub, FindMode.EverythingInSelfAndAncestors ) )
			{
				// Let's roll our own direction; tr.Direction will return Vector3.Zero if we're too close to hitGrub.
				var direction = (grub.Transform.Position - hitGrub.Transform.Position).ClampLength( 1f ) * -1f;
				hitGrub.Transform.Position += direction * 3f; // Prevent being stuck in Equipment.Grub
				hitGrub.CharacterController.Punch( (direction + Vector3.Up) * HitForce );
				hitGrub.CharacterController.ReleaseFromGround();
			}

			if ( tr.GameObject.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelfAndAncestors ) )
			{
				body.ApplyImpulseAt( tr.HitPosition, tr.Direction * HitForce * body.PhysicsBody.Mass );
			}

			if ( tr.GameObject.Components.TryGet( out Health health, FindMode.EverythingInAncestors ) )
				health.TakeDamage( GrubsDamageInfo.FromMelee( Damage, Equipment.Grub.Id, Equipment.Grub.Name, tr.HitPosition ) );
		}

		TimeSinceLastUsed = 0f;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( Equipment?.Grub is not { } grub )
			return;

		Gizmo.Transform = global::Transform.Zero.WithScale( 1f );

		var pc = grub.PlayerController;
		Gizmo.Draw.LineThickness = 2f;
		var startPos = grub.Transform.Position + Vector3.Up * 24f + HitOffset;
		Gizmo.Draw.Line( startPos, startPos + pc.Facing * pc.EyeRotation.Forward * HitSize.x );
	}
}
