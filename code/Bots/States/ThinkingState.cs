using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;
public partial class ThinkingState : BaseState
{
	const string CogsPath = "models/bots/thinking_cogs.vmdl";

	[Net]
	ModelEntity CogsModel { get; set; }

	float TimeToThink;

	public override void Simulate()
	{
		base.Simulate();

		if ( CogsModel is null )
		{
			CogsModel = new ModelEntity( CogsPath );
			CogsModel.Scale = 0.75f;
		}
		else
		{
			CogsModel.Position = MyPlayer.ActiveGrub.Position;
		}

		if ( Brain.TimeSinceStateStarted > TimeToThink )
		{
			FinishedState();
		}
	}

	public override void StartedState()
	{
		base.StartedState();
		TimeToThink = Game.Random.Float( 1f, 5f );
		MyPlayer.MoveInput = 0f;
		MyPlayer.LookInput = 0f;
	}

	public override void FinishedState()
	{
		base.FinishedState();

		if ( CogsModel is not null )
		{
			CogsModel.Delete();
		}
	}
}
