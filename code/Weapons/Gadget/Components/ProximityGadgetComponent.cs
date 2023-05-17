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
		Gadget.Tags.Add( "trigger" );
		Gadget.EnableTouchPersists = true;

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

		if ( _timeUntilArm && !_isArmed )
		{
			_isArmed = true;
			Gadget.SetMaterialGroup( 0 );
			Gadget.PlayScreenSound( "beep" );
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

	public override void Touch( Entity other )
	{
		Log.Info( "called" );

		if ( !_isArmed || _isTriggered || other is not Grub grub || grub.LifeState != LifeState.Alive )
			return;

		_isTriggered = true;
		_timeUntilExplode = ExplodeAfter;
	}
}
