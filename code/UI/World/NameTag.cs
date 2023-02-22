namespace Grubs;

/// <summary>
/// NameTag is a WorldPanel that can appear above any Entity.
/// </summary>
public class NameTag : WorldPanel
{
	public Entity Entity { get; private set; }

	public string EntityName => Entity.Name;
	public string EntityHealth => Entity.Health.CeilToInt().ToString();

	private Label _nameLabel { get; set; }
	private Label _healthLabel { get; set; }

	public NameTag( Entity entity )
	{
		Entity = entity;

		StyleSheet.Load( "UI/World/NameTag.scss" );

		_nameLabel = Add.Label( "Name", "ent-name" );
		_nameLabel.Bind( "text", this, nameof( EntityName ) );

		_healthLabel = Add.Label( "0", "ent-health" );
		_healthLabel.Bind( "text", this, nameof( EntityHealth ) );

		const float width = 600;
		const float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;
	}

	public override void Tick()
	{
		if ( !Entity.IsValid() )
		{
			Delete();
			return;
		}

		SetClass( "hidden", Entity.LifeState == LifeState.Dead );

		Position = Entity.Position + (Vector3.Up * 52f);
		Rotation = Rotation.LookAt( Vector3.Right );

		if ( Entity is not INameTag nameTaggedEntity )
			return;

		Style.FontColor = nameTaggedEntity.Color;

		WorldScale = (1.0f + Game.LocalClient.Components.Get<CameraMode>().DistanceRange.LerpInverse( Camera.Position.y )) * 3f;
	}
}
