using Sandbox;
using System.Collections.Generic;
using System.Linq;
using TerryForm.Pawn;

namespace TerryForm.UI.World
{
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
		}
	}
}
