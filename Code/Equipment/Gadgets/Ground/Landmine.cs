namespace Grubs.Equipment.Gadgets.Ground;

[Title( "Grubs - Landmine" ), Category( "Equipment" )]
public class Landmine : ProximityExplosive
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

	public override void OnTrigger()
	{
		base.OnTrigger();

		Sound.Play( BeepSound );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsDetonating && IsArmed )
		{
			if ( _skinToggle >= 0.1f )
			{
				Model.MaterialGroup = Model.MaterialGroup == _lightOnSkin ? _lightOffSkin : _lightOnSkin;
				_skinToggle = 0f;
			}
		}
	}
}
