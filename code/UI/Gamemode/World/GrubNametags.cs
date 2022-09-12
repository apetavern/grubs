using Grubs.Player;

namespace Grubs.UI.World;

public sealed class GrubNametags
{
	private Dictionary<Grub, GrubNametag> Nametags { get; } = new();

	public GrubNametags()
	{
		Event.Register( this );
		Update();
	}

	private void Update()
	{
		foreach ( var grub in Entity.All.OfType<Grub>() )
		{
			if ( Nametags.ContainsKey( grub ) )
				continue;

			Nametags.Add( grub, new GrubNametag( grub ) );
		}
	}

	[Event.Tick.Client]
	public void Frame()
	{
		Update();

		var invalidNametags = new List<Grub>();
		foreach ( var (grub, nametag) in Nametags )
		{
			if ( !grub.IsValid )
			{
				nametag.Delete();
				invalidNametags.Add( grub );
				continue;
			}

			nametag.AddClass( $"team-{nametag.Grub.Team.TeamName}" );
		}

		foreach ( var invalidNametag in invalidNametags )
			Nametags.Remove( invalidNametag );
	}
}
