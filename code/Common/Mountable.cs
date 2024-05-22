using Grubs.Equipment.Weapons;
using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Mountable" ), Category( "Grubs" )]
public sealed class Mountable : Component
{
	public Grub Grub { get; private set; }

	public void Mount( Grub grub )
	{
		Grub = grub;
		foreach ( var collider in Grub.Components.GetAll<Collider>( FindMode.EverythingInSelfAndChildren ) )
		{
			collider.Enabled = false;
		}
		Grub.PlayerController.IsOnRope = true;
		Grub.PlayerController.Enabled = false;
	}

	public void Dismount()
	{
		foreach ( var collider in Grub.Components.GetAll<Collider>( FindMode.EverythingInSelfAndChildren ) )
		{
			collider.Enabled = true;
		}
		Grub.Animator.GrubRenderer.Set( "heightdiff", 0f );
		Grub.Animator.GrubRenderer.Set( "aimangle", 0f );
		Grub.Animator.GrubRenderer.Set( "onrope", false );
		Grub.Transform.Rotation = Rotation.LookAt( Grub.Transform.Rotation.Forward.WithZ( 0 ), Vector3.Up );
		Grub.PlayerController.Enabled = true;
		Grub.PlayerController.IsOnRope = false;
		Grub.CharacterController.Punch(Components.Get<Rigidbody>().Velocity);
		Grub = null;
		GameObject.Destroy();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Grub is null )
			return;

		Grub.Transform.Position = Transform.Position;
	}
}
