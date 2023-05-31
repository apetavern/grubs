﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Grubs.Bots.States;
public partial class FiringState : BaseState
{
	bool Firing;

	public override void Simulate()
	{
		base.Simulate();
		AimAtTarget();
		FireAtTarget();
	}

	public void AimAtTarget()
	{
		var activeGrub = MyPlayer.ActiveGrub;

		MyPlayer.MoveInput = 0f;

		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		float distance = direction.Length;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );

		if ( !facingTarget || MyPlayer.ActiveGrub.Position.x.AlmostEqual( Brain.TargetGrub.Position.x, 50f ) )
		{
			MyPlayer.MoveInput = MathF.Sign( direction.Normal.x * 2f );
		}

		MyPlayer.LookInput = Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal );
	}

	public void FireAtTarget()
	{

		if ( Brain.TimeSinceStateStarted < 5f )
		{
			if ( MyPlayer.ActiveGrub.ActiveWeapon != null && MyPlayer.ActiveGrub.ActiveWeapon.CurrentUses < MyPlayer.ActiveGrub.ActiveWeapon.Charges )
			{
				if ( MyPlayer.ActiveGrub.ActiveWeapon.Charges > 1 )
				{
					if ( Brain.TimeSinceStateStarted % 3 == 1 )
					{
						Firing = !Firing;
						foreach ( var comp in MyPlayer.ActiveGrub.ActiveWeapon.Components.GetAll<WeaponComponent>() )
						{
							comp.IsFiring = Firing;
						}
						Input.SetAction( "fire", true );
					}
					else
					{
						foreach ( var comp in MyPlayer.ActiveGrub.ActiveWeapon.Components.GetAll<WeaponComponent>() )
						{
							comp.IsFiring = false;
						}
						Input.SetAction( "fire", false );
					}

				}
				else
				{
					if ( MyPlayer.ActiveGrub.ActiveWeapon.FiringType == FiringType.Instant && !Firing )
					{
						foreach ( var comp in MyPlayer.ActiveGrub.ActiveWeapon.Components.GetAll<WeaponComponent>() )
						{
							comp.IsFiring = true;
						}
						Firing = true;
					}

					HitScanComponent hitscan = MyPlayer.ActiveGrub.ActiveWeapon.Components.Get<HitScanComponent>();

					if ( hitscan == null || (hitscan != null && !hitscan.AutoMove) )
					{
						if ( Brain.TimeSinceStateStarted % 2 == 1 )
						{
							Firing = false;
						}
					}

					if ( MyPlayer.ActiveGrub.ActiveWeapon.FiringType == FiringType.Cursor && !Firing )
					{
						MyPlayer.MousePosition = Brain.TargetGrub.Position;

						foreach ( var comp in MyPlayer.ActiveGrub.ActiveWeapon.Components.GetAll<WeaponComponent>() )
						{
							comp.Fire();
						}

						Firing = true;
					}

					Input.SetAction( "fire", true );
				}
			}
		}
		else
		{
			Input.SetAction( "fire", false );
		}

		if ( Brain.TimeSinceStateStarted > 5f || (MyPlayer.ActiveGrub.IsValid && MyPlayer.ActiveGrub.ActiveWeapon.IsValid && MyPlayer.ActiveGrub.ActiveWeapon.CurrentUses >= MyPlayer.ActiveGrub.ActiveWeapon.Charges) )
		{
			Firing = false;
			FinishedState();
		}
	}
}
