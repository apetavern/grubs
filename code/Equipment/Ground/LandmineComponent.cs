namespace Grubs.Equipment.Ground;

[Title( "Grubs - Landmine" ), Category( "Equipment" )]
public class LandmineComponent : ProximityExplosiveComponent
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] public GameObject Landmine { get; set; }
	[Property, ResourceType( "sound" )] private string BeepSound { get; set; } = "";

	private const string _lightOnSkin = "default";
	private const string _lightOffSkin = "lightoff";
	private TimeSince _skinToggle = 0f;

	public override void OnArm()
	{
		base.OnArm();

		Model.MaterialGroup = _lightOnSkin;
	}

	public override void OnExplode()
	{
		Landmine.Destroy();

		base.OnExplode();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Transform.Rotation = Rotation.From(
			Transform.Rotation.Pitch().Clamp( -45, 45 ),
			Transform.Rotation.Yaw().Clamp( -45, 45 ),
			Transform.Rotation.Roll()
			);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsDetonating )
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
