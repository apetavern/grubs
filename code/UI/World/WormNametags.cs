using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametags
{
	private Dictionary<Entity, WormNametag> Nametags { get; set; } = new();

	public WormNametags()
	{
		Event.Register( this );
		Update();
	}

	public void Update()
	{
		if ( Host.IsClient )
		{
			foreach ( var worm in Entity.All.Where( e => e is Worm ) )
			{
				if ( !Nametags.ContainsKey( worm ) )
				{
					var nametag = new WormNametag();
					nametag.Worm = worm as Worm;

					Nametags.Add( worm, nametag );
				}
			}
		}
	}

	[Event.Tick]
	public void OnTick()
	{
		Update();

		foreach ( var nametag in Nametags.Values )
		{
			var teamClass = nametag.Worm.GetTeamName();
			nametag.AddClass( $"team-{teamClass}" );
		}
	}
}
