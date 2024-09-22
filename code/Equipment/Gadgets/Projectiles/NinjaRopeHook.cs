using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Gamemodes;
using Grubs.Helpers;

namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Ninja Rope Hook Tip" ), Category( "Equipment" )]
public sealed class NinjaRopeHook : Component, Component.ICollisionListener
{
	[Property] public GameObject MountObject { get; set; }

	[Property] public PhysicsProjectile PhysicsProjectileComponent { get; set; }

	private RopeBehavior Rope { get; set; }
	private TimeSince _timeSinceCreated = 0f;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( !MountObject.IsValid() || Rope == null && _timeSinceCreated > NinjaRopeWeapon.Timeout )
		{
			GameObject.Destroy();
		}
		else if ( MountObject.Enabled && Rope != null )
		{
			var mountable = MountObject.Components.Get<Mountable>();

			if ( PhysicsProjectileComponent.Grub.IsValid() && mountable.MountEnabled )
			{
				var grub = PhysicsProjectileComponent.Grub;

				grub.PlayerController.EyeRotation = Rotation.FromPitch( MathF.Abs( Vector3.GetAngle( grub.Transform.Rotation.Forward, Rope.HookDirection ) - 15f ) * -grub.PlayerController.Facing );
				grub.PlayerController.IsOnRope = true;
				grub.Transform.Rotation = Rotation.Lerp( PhysicsProjectileComponent.Grub.Transform.Rotation, Rotation.LookAt( Rope.HookDirection ) * Rotation.FromPitch( 45f ), Time.Delta * 10f );
				grub.CharacterController.IsOnGround = false;
				grub.CharacterController.Velocity = Vector3.Zero;
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

		Transform.Position = other.Contact.Point - other.Contact.Normal * 5f;
		Transform.Rotation = Rotation.LookAt( other.Contact.Normal );
		CreateRopeSystem();
	}

	public void CreateRopeSystem()
	{
		MountObject.Parent = Scene;
		MountObject.Transform.Position = PhysicsProjectileComponent.Grub.Transform.Position;
		MountObject.Enabled = true;
		Rope = MountObject.Components.Get<RopeBehavior>();
		MountObject.Components.Get<Mountable>().Mount( PhysicsProjectileComponent.Grub );
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
		       || !PhysicsProjectileComponent.Grub.IsActive 
		       || Gamemode.GetCurrent().TurnIsChanging;
	}
}
