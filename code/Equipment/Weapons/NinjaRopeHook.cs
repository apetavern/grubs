namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Ninja Rope Hook Tip" ), Category( "Equipment" )]
public sealed class NinjaRopeHook : Component, Component.ICollisionListener
{
	public void OnCollisionStart( Collision other )
	{
		Log.Info( "Collision Start" );
		Log.Info( other.Other.GameObject.Tags );
	}

	public void OnCollisionUpdate( Collision other )
	{
		
	}

	public void OnCollisionStop( CollisionStop other )
	{
		
	}
}
