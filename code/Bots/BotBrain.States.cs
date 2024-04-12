using Grubs.Pawn;
using Sandbox;
using System.Threading.Tasks;

namespace Grubs.Bots;

public partial class BotBrain : Component
{
	[ActionGraphNode( "botbrain.base" ), Title( "Base State" ), Group( "GrubsBot" ), Icon( "hourglass_bottom" )]
	public async Task<BotBrain> BaseBrainState()
	{
		var executed = false;
		while ( !executed )
		{
			//Logic goes here
			await Task.Frame();
			//If we're done set Executed to true
			executed = true;
		}

		return this;
	}

	[ActionGraphNode( "botbrain.thinking" ), Title( "Thinking State" ), Group( "GrubsBot" ), Icon( "hourglass_bottom" )]
	public async Task<BotBrain> ThinkingState()
	{
		if ( ActiveGrub is null )
			return this;

		var executed = false;
		while ( !executed )
		{
			ActiveGrub.Components.Get<GrubAnimator>().Thinking = true;
			await Task.DelaySeconds( Game.Random.Float( 1f, 3f ) );
			ActiveGrub.Components.Get<GrubAnimator>().Thinking = false;
			executed = true;
		}

		return this;
	}
}
