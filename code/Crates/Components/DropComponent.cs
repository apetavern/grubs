namespace Grubs;

public class DropComponent : EntityComponent<Drop>
{
	public Drop Drop => Entity;

	public virtual void Simulate( IClient client )
	{
	}
}
