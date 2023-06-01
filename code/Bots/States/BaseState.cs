using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;
public partial class BaseState : EntityComponent<BotBrain>
{
	public Player MyPlayer => Entity.Owner as Player;

	public BotBrain Brain => Entity as BotBrain;

	public float MaxTimeInState = 10f;

	public virtual void Simulate()
	{
		if ( MyPlayer.ActiveGrub.HasBeenDamaged )
		{
			FinishedState();
		}
	}
	public virtual void StartedState()
	{

	}

	public virtual void FinishedState()
	{
		Brain.NextState();
	}
}
