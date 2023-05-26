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
			ClearUnusableWeapons();

			if ( BrainEnt is null || !BrainEnt.IsValid )
			{
				BrainEnt = new BotBrain();
				BrainEnt.Owner = MyPlayer;
			}

			TargetGrub = BrainEnt.TargetGrub;

			BrainEnt.SimulateCurrentState();
		}
		else
		{
			if ( BrainEnt is null || !BrainEnt.IsValid )
			{
				BrainEnt = new BotBrain();
				BrainEnt.Owner = MyPlayer;
			}
			TargetGrub = null;
			BrainEnt.TargetGrub = null;
			BrainEnt.CurrentState = 0;
			BrainEnt.TimeSinceStateStarted = 0f;
			Input.SetAction( "fire", false );
			Input.SetAction( "jump", false );
			Input.SetAction( "backflip", false );
		}
	}

	public void ClearUnusableWeapons()
	{
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
	}

	[ConCmd.Server( "gr_bot_add" )]
	public static void CreateGrubsBot( int count = 1 )
	{
		for ( int i = 0; i < count; i++ )
		{
			var bot = new GrubsBot();
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
