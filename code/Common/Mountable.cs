using Grubs.Equipment.Weapons;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Common;

[Title( "Grubs - Mountable" ), Category( "Grubs" )]
public sealed class Mountable : Component
{
	public Grub Grub { get; private set; }
	public bool MountEnabled { get; set; }

	public void Mount( Grub grub )
	{
		Grub = grub;
		foreach ( var collider in Grub.Components.GetAll<Collider>( FindMode.EverythingInSelfAndChildren ) )
		{
			collider.Enabled = false;
		}
		Grub.PlayerController.IsOnRope = true;
		Grub.PlayerController.Enabled = false;
		Grub.ActiveMountable = this;
		MountEnabled = true;
	}

	public void Dismount()
	{
		if ( !Grub.IsValid() || !Grub.Animator.IsValid() || !Grub.PlayerController.IsValid() || !Grub.CharacterController.IsValid() )
			return;

		foreach ( var collider in Grub.Components.GetAll<Collider>( FindMode.EverythingInSelfAndChildren ) )
		{
			collider.Enabled = true;
		}
		Grub.Animator.GrubRenderer.Set( "heightdiff", 0f );
		Grub.Animator.GrubRenderer.Set( "aimangle", 0f );
		Grub.Animator.GrubRenderer.Set( "onrope", false );
		Grub.WorldRotation = Rotation.LookAt( Grub.WorldRotation.Forward.WithZ( 0 ), Vector3.Up );
		Grub.PlayerController.Enabled = true;
		Grub.PlayerController.IsOnRope = false;
		Grub.CharacterController.Velocity = Components.Get<Rigidbody>().Velocity;
		MountEnabled = false;
		Grub.ActiveMountable = null;
		Grub = null;
		GameObject?.Destroy();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// kidd: We really shouldn't have to check if the transforms are null, but we're getting NRE spammed somehow.
		// Probably s&box issue, but don't know where to begin reproducing.
		if ( Grub.IsValid() && Grub.Transform is not null && Transform is not null )
			Grub.WorldPosition = WorldPosition;
	}
}
