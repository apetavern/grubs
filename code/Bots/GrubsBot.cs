using Sandbox;
using System.Linq;

namespace Grubs.Bots;

public partial class GrubsBot : Bot
{
	Grub TargetGrub { get; set; }

	Grub ActiveGrub => (Client.Pawn as Player).ActiveGrub;

	Player MyPlayer => Client.Pawn as Player;

	BotBrain BrainEnt;

	public override void BuildInput()
	{
		base.BuildInput();
		if ( MyPlayer.IsTurn )
		{
			if ( BrainEnt.MyPlayer != MyPlayer )
			{
				BrainEnt.MyPlayer = MyPlayer;
			}
			TargetGrub = BrainEnt.TargetGrub;

			if ( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "shot" ) ).Any() )
			{
				MyPlayer.Inventory.Weapons.Remove( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "shot" ) ).First() );
			}

			if ( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "para" ) ).Any() )
			{
				MyPlayer.Inventory.Weapons.Remove( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "para" ) ).First() );
			}

			if ( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "jetpack" ) ).Any() )
			{
				MyPlayer.Inventory.Weapons.Remove( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "jetpack" ) ).First() );
			}

			if ( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "torch" ) ).Any() )
			{
				MyPlayer.Inventory.Weapons.Remove( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "torch" ) ).First() );
			}

			BrainEnt.SimulateCurrentState();
		}
		else
		{
			TargetGrub = null;
			BrainEnt.TargetGrub = null;
			BrainEnt.CurrentState = 0;
			BrainEnt.TimeSinceStateStarted = 0f;
			Input.SetAction( "fire", false );
			Input.SetAction( "jump", false );
			Input.SetAction( "backflip", false );
		}
	}

	[ConCmd.Server( "grubs_bot_add" )]
	public static void CreateGrubsBot( int count = 1 )
	{
		for ( int i = 0; i < count; i++ )
		{
			var bot = new GrubsBot();

			var brains = new BotBrain();

			brains.Owner = bot.MyPlayer;

			bot.BrainEnt = brains;
		}
	}

	public override void Tick()
	{
		if ( (Client.Pawn as Player).IsTurn )
		{

		}
		else
		{
			TargetGrub = null;
		}
	}
}
