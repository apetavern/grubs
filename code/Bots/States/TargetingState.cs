using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;
public partial class TargetingState : BaseState
{

	public override void Simulate()
	{
		base.Simulate();
		FindTargetGrub();
	}

	public void LineOfSightTargetCheck()
	{
		var enemyGrubs = Sandbox.Entity.All.OfType<Grub>()
							.Where( G => G.Player != MyPlayer )
							.Where( G => G.LifeState != LifeState.Dead && G.LifeState != LifeState.Dying )
							.OrderBy( G => Vector3.DistanceBetween( G.Position, MyPlayer.ActiveGrub.Position ) );

		foreach ( var grub in enemyGrubs )
		{
			if ( Trace.Ray( MyPlayer.ActiveGrub.EyePosition - Vector3.Up * 10f, grub.EyePosition - Vector3.Up * 10f ).Ignore( MyPlayer.ActiveGrub ).Run().Entity == grub )
			{
				Brain.TargetGrub = grub;
			}
		}
	}

	public void FindTargetGrub()
	{
		if ( Brain.TargetGrub == null )
		{
			var enemyGrubs = Sandbox.Entity.All.OfType<Grub>()
								.Where( G => G.Player != MyPlayer )
								.Where( G => G.LifeState != LifeState.Dead && G.LifeState != LifeState.Dying )
								.OrderBy( G => Vector3.DistanceBetween( G.Position, MyPlayer.ActiveGrub.Position ) );



			Brain.TargetGrub = enemyGrubs.First();
		}
		else
		{
			DebugOverlay.Sphere( Brain.TargetGrub.Position, 10f, Color.Red );
			FinishedState();
		}
	}
}
