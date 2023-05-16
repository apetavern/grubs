namespace Grubs;

[Prefab]
public partial class ProximityGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public float ArmAfter { get; set; } = 4.0f;

	[Prefab, Net]
	public float TriggerRadius { get; set; } = 4.0f;

	[Prefab, Net]
	public float ExplodeAfter { get; set; } = 3.0f;

	private TimeUntil _timeUntilArm;
	private TimeUntil _timeUntilExplode;
	private TimeUntil _timeUntilNextBeep;
	private bool _isArmed;
	private bool _isTriggered;

	public override void Spawn()
	{
		_timeUntilArm = ArmAfter;
		_isArmed = false;
		_isTriggered = false;

		Gadget.SetMaterialGroup( 1 );
	}

	public override bool IsResolved()
	{
		return !_isTriggered || _timeUntilExplode;
	}

	public override void Simulate( IClient client )
	{
		if ( Game.IsClient )
			return;

		if ( _timeUntilArm && !_isTriggered )
		{
			if ( !_isArmed )
			{
				_isArmed = true;
				Gadget.SetMaterialGroup( 0 );
				Gadget.PlayScreenSound( "beep" );
			}

			_isTriggered = Sandbox.Entity.FindInSphere( Gadget.Position, TriggerRadius ).OfType<Grub>().Any();
			_timeUntilExplode = ExplodeAfter;
		}

		if ( _isTriggered )
		{
			Gadget.SetMaterialGroup( _timeUntilNextBeep ? 0 : 1 );

			if ( _timeUntilNextBeep )
			{
				Gadget.PlayScreenSound( "beep" );
				_timeUntilNextBeep = _timeUntilExplode / 4;
			}

			if ( _timeUntilExplode )
				Gadget.Components.Get<ExplosiveGadgetComponent>()?.Explode();
		}
	}
}
