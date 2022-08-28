using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametags
{
	private Dictionary<Worm, WormNametag> Nametags { get; } = new();

	public WormNametags()
	{
		Event.Register( this );
		Update();
	}

	private void Update()
	{
		foreach ( var worm in Entity.All.OfType<Worm>() )
		{
			if ( Nametags.ContainsKey( worm ) )
				continue;

			Nametags.Add( worm, new WormNametag( worm ) );
		}
	}

	[Event.Tick.Client]
	public void Frame()
	{
		Update();

		var invalidNametags = new List<Worm>();
		foreach ( var (worm, nametag) in Nametags )
		{
			if ( worm is null || !worm.IsValid )
			{
				nametag.Delete();
				invalidNametags.Add( worm );
				continue;
			}

			nametag.AddClass( $"team-{nametag.Worm.Team.TeamName}" );
		}

		foreach ( var invalidNametag in invalidNametags )
			Nametags.Remove( invalidNametag );
	}
}
