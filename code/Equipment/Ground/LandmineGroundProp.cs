namespace Grubs.Equipment.Ground;

[Title( "Grubs - Landmine" ), Category( "Equipment" )]
public class LandmineGroundProp : ProximityExplosiveComponent
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property, ResourceType( "sound" )] private string BeepSound { get; set; } = "";

	private const string _lightOnSkin = "default";
	private const string _lightOffSkin = "lightoff";
	private TimeSince _skinToggle = 0f;

	public override void OnArm()
	{
		base.OnArm();

		Model.MaterialGroup = _lightOnSkin;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Transform.Rotation = Rotation.From(
			Transform.Rotation.Pitch().Clamp( -45, 45 ),
			Transform.Rotation.Yaw().Clamp( -45, 45 ),
			Transform.Rotation.Roll().Clamp( -45, 45 )
			);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (IsDetonating)
		{
			if ( _skinToggle >= 0.5f )
			{
				Model.MaterialGroup = Model.MaterialGroup == _lightOnSkin ? _lightOffSkin : _lightOnSkin;
				_skinToggle = 0f;
				Sound.Play( BeepSound );
			}
		}
	}
}
