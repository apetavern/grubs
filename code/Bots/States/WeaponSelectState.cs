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

	public void DecideWeapon()
	{
		var activeGrub = MyPlayer.ActiveGrub;

		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 3f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		if ( !lineOfSight )
		{
			HitScanComponent hitscancomp;
			var selectedWeapon = MyPlayer.Inventory.Weapons.Where( W => W.Ammo > 0 ).Where( W => W.FiringType == FiringType.Instant ).Where( W => W.Components.TryGet( out hitscancomp ) == true ).OrderBy( x => Game.Random.Int( 1000 ) ).FirstOrDefault();

			if ( selectedWeapon == null )//We ran out of trace weapons...
			{
				GadgetWeaponComponent gadgetcomp = null;
				selectedWeapon = MyPlayer.Inventory.Weapons.Where( W => W.Ammo > 0 ).Where( W => W.FiringType == FiringType.Charged ).Where( W => W.Components.TryGet( out gadgetcomp ) == true ).OrderBy( x => Game.Random.Int( 1000 ) ).FirstOrDefault();
			}

			MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}
		else
		{
			GadgetWeaponComponent gadgetcomp = null;
			var selectedWeapon = MyPlayer.Inventory.Weapons.Where( W => W.Ammo > 0 ).Where( W => W.FiringType == FiringType.Charged ).Where( W => W.Components.TryGet( out gadgetcomp ) == true ).OrderBy( x => Game.Random.Int( 1000 ) ).FirstOrDefault();

			MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}

		if ( MyPlayer.ActiveGrub.ActiveWeapon == null )
		{
			var selectedWeapon = MyPlayer.Inventory.Weapons.Where( W => W.Ammo > 0 ).OrderBy( x => Game.Random.Int( 1000 ) ).FirstOrDefault();

			MyPlayer.Inventory.SetActiveWeapon( selectedWeapon );
		}


		FinishedState();
	}

}
