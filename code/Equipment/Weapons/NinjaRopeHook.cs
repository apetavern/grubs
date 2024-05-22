using Grubs.Common;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Ninja Rope Hook Tip" ), Category( "Equipment" )]
public sealed class NinjaRopeHook : Component, Component.ICollisionListener
{
	[Property] public GameObject MountObject { get; set; }

	[Property] public PhysicsProjectileComponent PhysicsProjectileComponent { get; set; }

	RopeBehaviorComponent Rope;

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if( !MountObject.IsValid() )
		{
			GameObject.Destroy();
		}else if ( MountObject.Enabled && Rope != null)
		{
			if ( PhysicsProjectileComponent.Grub.IsValid() )
			{
				var grub = PhysicsProjectileComponent.Grub;
				grub.Animator.GrubRenderer.Set( "heightdiff", 15f );
				grub.Animator.GrubRenderer.Set( "aimangle", Vector3.GetAngle( grub.Transform.Rotation.Forward, Rope.HookDirection) - 15f );
				grub.PlayerController.IsOnRope = true;
				Log.Info( grub.PlayerController.ShouldShowWeapon() );
				grub.Transform.Rotation = Rotation.Lerp( PhysicsProjectileComponent.Grub.Transform.Rotation, Rotation.LookAt( Rope.HookDirection ) * Rotation.FromPitch(45f), Time.Delta * 10f);
				grub.CharacterController.IsOnGround = false;
			}
			else
			{
				MountObject.Components.Get<Mountable>().Dismount();
			}
		}
	}

	public void OnCollisionStart( Collision other )
	{
		Components.Get<Rigidbody>().Enabled = false;

		Transform.Position = other.Contact.Point - other.Contact.Normal * 5f;
		Transform.Rotation = Rotation.LookAt( other.Contact.Normal );
		CreateRopeSystem();
	}

	public void CreateRopeSystem()
	{
		MountObject.Parent = Scene;
		MountObject.Transform.Position = PhysicsProjectileComponent.Grub.Transform.Position;
		MountObject.Enabled = true;
		Rope = MountObject.Components.Get<RopeBehaviorComponent>();
		MountObject.Components.Get<Mountable>().Mount( PhysicsProjectileComponent.Grub );
	}

	public void OnCollisionUpdate( Collision other )
	{
		
	}

	public void OnCollisionStop( CollisionStop other )
	{
		
	}
}
