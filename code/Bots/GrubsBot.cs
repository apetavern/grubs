namespace Grubs.Bots;

public partial class GrubsBot : Bot
{
	static GrubsBot()
	{
		SetDefaultNames( BotNames );
	}

	Grub TargetGrub { get; set; }

	Grub ActiveGrub => (Client.Pawn as Player).ActiveGrub;

	Player MyPlayer => Client.Pawn as Player;

	BotBrain BrainEnt;


	static List<string> BotNames = new List<string>
	{
		"[BOT] El Jabroga",
		"[BOT] Clyde",
		"[BOT] Mike Oxlong",
		"[BOT] Null Reference",
		"[BOT] Chip Danger",
		"[BOT] RealBigSnorris",
		"[BOT] BigJohnBorris",
		"[BOT] Melty Chihuahua",
		"[BOT] Blitz Command",
		"[BOT] Scorch Shot",
		"[BOT] Toxic Viper",
		"[BOT] Snipe Hawk",
		"[BOT] Chaos Fury",
		"[BOT] Blast Engine",
		"[BOT] Meteor Blitz",
		"[BOT] Annihilate X",
		"[BOT] Rapid Marksman",
		"[BOT] Shadow Stalker",
		"[BOT] Doombringer",
		"[BOT] Avalanche Strike",
		"[BOT] Wreck Bot",
		"[BOT] Venomous Fang",
		"[BOT] Destroyer",
		"[BOT] Sizzle Blast",
		"[BOT] Blade Master",
		"[BOT] Pyro",
	};

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
			if ( Game.Clients.Count >= Game.Server.MaxPlayers )
			{
				Log.Warning( "Cannot add bot - max players already reached." );
				return;
			}

			_ = new GrubsBot
			{
				TurnedOffAntenna = true
			};
		}
	}

	List<ModelEntity> Antenna = new List<ModelEntity>();

	public bool TurnedOffAntenna;

	ModelEntity LastAntennaActivated;

	public override void Tick()
	{
		if ( (Client.Pawn as Player).IsTurn )
		{
			if ( TurnedOffAntenna )
			{
				foreach ( var item in MyPlayer.ActiveGrub.Children )
				{
					if ( item.Tags.Has( "antenna" ) )
					{
						(item as ModelEntity).SetMaterialGroup( "active" );
						LastAntennaActivated = item as ModelEntity;
					}
				}
				TurnedOffAntenna = false;
			}


		}
		else
		{
			if ( !TurnedOffAntenna && LastAntennaActivated != null )
			{
				LastAntennaActivated.SetMaterialGroup( "standard" );

				TurnedOffAntenna = true;
			}

			//Sloppy initialization of antenna models, this script stays around when the game ends but your grubs don't so I gotta re-check and re-init every round.
			if ( MyPlayer.Grubs.Count > 0 && (Antenna.Count == 0 || Antenna[0] == null || !Antenna[0].IsValid) )
			{
				foreach ( var anten in Antenna )
				{
					if ( anten != null )
					{
						anten.Delete();
					}
				}

				Antenna.Clear();

				foreach ( var grub in MyPlayer.Grubs )
				{
					var antenna = new ModelEntity( "models/cosmetics/bot_antenna/bot_antenna.vmdl" );
					Antenna.Add( antenna );
					antenna.Tags.Add( "antenna" );
					antenna.SetParent( grub, true );
				}
			}

			TargetGrub = null;
		}
	}
}
