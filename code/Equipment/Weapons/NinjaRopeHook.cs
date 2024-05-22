using Grubs.Common;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Ninja Rope Hook Tip" ), Category( "Equipment" )]
public sealed class NinjaRopeHook : Component, Component.ICollisionListener
{
	[Property]public GameObject MountObject { get; set; }

	[Property] public PhysicsProjectileComponent PhysicsProjectileComponent { get; set; }

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if( !MountObject.IsValid() )
		{
			GameObject.Destroy();
		}
	}

	public void OnCollisionStart( Collision other )
	{
		Components.Get<Rigidbody>().Enabled = false;

		CreateRopeSystem();
		//Log.Info( "Collision Start" );
		//Log.Info( other.Other.GameObject.Tags );
	}

	public void CreateRopeSystem()
	{
		MountObject.Parent = Scene;
		MountObject.Transform.Position = PhysicsProjectileComponent.Grub.Transform.Position;
		MountObject.Enabled = true;
		MountObject.Components.Get<Mountable>().Mount( PhysicsProjectileComponent.Grub );
	}

	public void OnCollisionUpdate( Collision other )
	{
		
	}

	public void OnCollisionStop( CollisionStop other )
	{
		
	}
}
