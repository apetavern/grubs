using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Helpers;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Ninja Rope Hook Tip" ), Category( "Equipment" )]
public sealed class NinjaRopeHook : Component, Component.ICollisionListener
{
	// The GameObject that holds the Mountable component.
	[Property] public GameObject MountObject { get; set; }
	
	// The PhysicsProjectile component for the Hook Tip.
	[Property] public PhysicsProjectile PhysicsProjectileComponent { get; set; }
	
	// The Spring Joint between this hook tip and the mountable.
	[Property] public SpringJoint SpringJoint { get; set; }
	
	// The rope behaviour component.
	[Property] public RopeBehavior RopeBehavior { get; set; }
	
	private TimeSince TimeSinceCreated { get; } = 0f;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( !MountObject.IsValid() || !RopeBehavior.IsValid() && TimeSinceCreated > NinjaRopeWeapon.TimeOut )
		{
			GameObject.Destroy();
		}
		else if ( MountObject.Enabled && RopeBehavior.IsValid() )
		{
			var mountable = MountObject.Components.Get<Mountable>();
			if ( !mountable.IsValid() )
				return;
		
			if ( PhysicsProjectileComponent.Grub.IsValid() && mountable.MountEnabled )
			{
				var grub = PhysicsProjectileComponent.Grub;
		
				grub.PlayerController.EyeRotation = Rotation.FromPitch( MathF.Abs( Vector3.GetAngle( grub.WorldRotation.Forward, RopeBehavior.HookDirection ) - 15f ) * -grub.PlayerController.Facing );
				grub.PlayerController.IsOnRope = true;
				grub.WorldRotation = Rotation.Lerp( PhysicsProjectileComponent.Grub.WorldRotation, Rotation.LookAt( RopeBehavior.HookDirection ) * Rotation.FromPitch( 45f ), Time.Delta * 10f );
				grub.CharacterController.IsOnGround = false;
				grub.CharacterController.SetVelocity( Vector3.Zero );
			}
			else
			{
				if ( !MountObject.IsValid() )
					return;
		
				if ( !mountable.IsValid() )
					return;
		
				mountable.Dismount();
			}
		}
	}

	public void OnCollisionStart( Collision other )
	{
		if ( ShouldDestroySelf() )
		{
			GameObject.Destroy();
			return;
		}

		var rb = Components.Get<Rigidbody>();
		if ( rb.IsValid() )
			rb.Enabled = false;

		WorldPosition = other.Contact.Point - other.Contact.Normal * 5f;
		WorldRotation = Rotation.LookAt( other.Contact.Normal );
		
		SetupRope();
	}

	private void SetupRope()
	{
		MountObject.Enabled = true;
		SpringJoint.Enabled = true;

		var rope = MountObject.GetComponent<RopeBehavior>( true );
		rope.Grub = PhysicsProjectileComponent.Grub;
		
		rope.Enabled = true;
	}

	public void OnCollisionUpdate( Collision other )
	{

	}

	public void OnCollisionStop( CollisionStop other )
	{

	}

	private bool ShouldDestroySelf()
	{
		return !PhysicsProjectileComponent.IsValid()
		       || !PhysicsProjectileComponent.Grub.IsValid()
		       || !PhysicsProjectileComponent.Grub.IsActive();
		// || Gamemode.GetCurrent().TurnIsChanging;
	}
}
