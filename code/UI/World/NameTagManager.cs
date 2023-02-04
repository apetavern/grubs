namespace Grubs;

public class NameTagManager
{
	public Dictionary<Entity, NameTag> NameTags { get; } = new();

	public NameTagManager()
	{
		Event.Register( this );

		Update();
	}

	private void Update()
	{
		foreach ( var ent in Entity.All.OfType<Entity>().Where( e => e is INameTag ) )
		{
			if ( NameTags.ContainsKey( ent ) )
				continue;

			NameTags.Add( ent, new NameTag( ent ) );
		}
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		Update();

		var invalidNameTags = new List<Entity>();
		foreach ( var (entity, nameTag) in NameTags )
		{
			if ( !entity.IsValid() )
			{
				nameTag.Delete();
				invalidNameTags.Add( entity );
				continue;
			}
		}

		foreach ( var invalid in invalidNameTags )
		{
			NameTags.Remove( invalid );
		}
	}
}
