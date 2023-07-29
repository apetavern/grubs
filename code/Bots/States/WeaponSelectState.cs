using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;
public partial class WeaponSelectState : BaseState
{
	public override void Simulate()
	{
		base.Simulate();
		DecideWeapon();
	}

	List<string> LineOfSightWeapons = new List<string>()
	{
		"bazooka",
		"grenade",
		"cluster grenade",
		"revolver",
		"shotgun",
		"petrol bomb",
		"uzi",
		"gibgun",
		"minigun",
		"flamethrower",
	};

	List<string> LandPenetratingWeapons = new List<string>()
	{
		"uzi",
		"gibgun",
		"minigun",
		"torch",
		"flamethrower",
	};

	List<string> ClosebyWeapons = new List<string>()
	{
		"mine",
		"dynamite",
		"baseball bat",
		"bitch slap"
	};

	List<string> DroppableProjectileWeapons = new List<string>()
	{
		"mine",
		"dynamite"
	};

	List<string> FarReachWeapons = new List<string>()
	{
		"teleporter",
		"concrete garry"
	};

	public void DecideWeapon()
	{
		var activeGrub = MyPlayer.ActiveGrub;

		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 5f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		var availableWeapons = MyPlayer.Inventory.Weapons.Where( w => w.IsAvailable() ).OrderBy( x => Game.Random.Int( 1000 ) );

		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 15f + Vector3.Up * 5f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 512f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool OnEdge = clifftr.Distance > BotBrain.MaxFallDistance || !clifftr.Hit || MathF.Round( clifftr.EndPosition.z ) == 0;

		if ( !lineOfSight )
		{
			var selectedWeapon = availableWeapons.Where( W => LandPenetratingWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();

			if ( selectedWeapon == null )//We ran out of penetrate weapons...
			{
				selectedWeapon = availableWeapons.Where( W => LineOfSightWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();
			}

			MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}
		else
		{
			var selectedWeapon = availableWeapons.Where( W => LineOfSightWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();

			MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}

		if ( direction.z > 128 && OnEdge )
		{
			var selectedWeapon = availableWeapons.Where( W => DroppableProjectileWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();

			if ( selectedWeapon != null )
				MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}

		if ( distance < 64f )
		{
			var selectedWeapon = availableWeapons.Where( W => ClosebyWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();

			if ( selectedWeapon != null )
				MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}

		if ( (distance > 400f || direction.z < -256f) && !lineOfSight )
		{
			var selectedWeapon = availableWeapons.Where( W => FarReachWeapons.Contains( W.Name.ToLower() ) ).FirstOrDefault();

			if ( selectedWeapon != null )
				MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}

		FinishedState();
	}

}
