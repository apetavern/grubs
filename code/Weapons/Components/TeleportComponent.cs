namespace Grubs;

[Prefab]
public partial class TeleportComponent : WeaponComponent
{
	[Net]
	public Vector2 MousePosition { get; set; }

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

		Event.Run( GrubsEvent.Player.PointerEventChanged, false );
	}
}
