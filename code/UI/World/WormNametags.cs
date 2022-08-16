using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametags
{
	private Dictionary<Entity, WormNametag> Nametags { get; } = new();

	public WormNametags()
	{
		Event.Register( this );
		Update();
	}

	private void Update()
	{
		foreach ( var worm in Entity.All.Where( e => e is Worm ) )
		{
			if ( Nametags.ContainsKey( worm ) )
				continue;
			
			Nametags.Add( worm, new WormNametag {Worm = worm as Worm} );
		}
	}

	[Event.Tick.Client]
	public void Frame()
	{
		Update();

		foreach ( var nametag in Nametags.Values )
		{
			var teamClass = nametag.Worm.GetTeamName();
			nametag.AddClass( $"team-{teamClass}" );
		}
	}
}
