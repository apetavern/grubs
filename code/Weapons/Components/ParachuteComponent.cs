using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class ParachuteComponent : WeaponComponent
{
	[Net, Predicted]
	private float Airtime { get; set; } = 0.3f;

	[Net, Predicted]
	private bool Deployed { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		/*daif ( !Grub.Controller.IsGrounded )
		{
			Airtime -= Time.Delta;
		}

		if ( Airtime <= 0f )
		{
			Deployed = true;
			Weapon.SetAnimParameter( "landed", false );
			Weapon.SetAnimParameter( "deploy", true );
		}*/

		if ( IsFiring )
		{
			Fire();
		}

		if ( Deployed )
		{
			Grub.Velocity = new Vector3( Grub.Velocity.x - Player.MoveInput + GamemodeSystem.Instance.ActiveWindForce, Grub.Velocity.y, Grub.Velocity.ClampLength( 75f ).z );
		}

		if ( Grub.Controller.IsGrounded && Deployed )
		{
			Weapon.SetAnimParameter( "deploy", false );
			Weapon.SetAnimParameter( "landed", true );
			FireFinished();
			Deployed = false;
		}
	}

	public override void FireInstant()
	{
		Deployed = true;
		Weapon.SetAnimParameter( "landed", false );
		Weapon.SetAnimParameter( "deploy", true );
		base.FireInstant();
	}

	public override void FireFinished()
	{
		base.FireFinished();
		Airtime = 0.3f;
		IsFiring = false;
	}
}
