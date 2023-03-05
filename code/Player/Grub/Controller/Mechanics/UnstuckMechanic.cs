namespace Grubs;

public class UnstuckMechanic : ControllerMechanic
{
	private int _stuckTries = 0;

	protected override bool ShouldStart()
	{
		return Grub.LifeState != LifeState.Dead;
	}

	protected override void Simulate()
	{
		var result = Controller.TraceBBox( Position, Position );

		// Not stuck
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return;
		}

		if ( Game.IsClient )
			return;

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Position + Vector3.Random.Normal.WithY( 0 ) * (_stuckTries / 2.0f);

			if ( i == 0 )
				pos = Position + Vector3.Up * 5;

			result = Controller.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Position = pos;
				return;
			}
		}

		_stuckTries++;
	}
}
