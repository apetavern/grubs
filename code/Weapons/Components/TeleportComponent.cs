namespace Grubs;

[Prefab]
public partial class TeleportComponent : WeaponComponent
{
	[Net]
	public Vector2 MousePosition { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring )
			Fire();
	}

	public override void FireCursor()
	{
		IsFiring = false;
		Grub.Position = Grub.Player.MousePosition;

		FireFinished();
	}

	public override void FireFinished()
	{
		base.FireFinished();

		if ( Game.IsClient )
			Event.Run( "pointer.disabled" );
	}
}
