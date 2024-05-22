using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Mountable" ), Category( "Grubs" )]
public sealed class Mountable : Component
{
	public Grub Grub { get; private set; }

	public void Mount( Grub grub )
	{
		Grub = grub;
	}

	public void Dismount()
	{
		Grub = null;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Grub is null )
			return;

		Grub.Transform.Position = Transform.Position;
	}
}
