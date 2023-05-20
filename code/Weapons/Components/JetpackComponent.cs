using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{

	[Prefab, Net]
	public float ThrustSpeed { get; set; } = 2;

	[Net]
	public Vector3 VelocityInput { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		Grub.SetAnimParameter( "jetpack_active", true );
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * VelocityInput.x / 5f );

		VelocityInput = new Vector3( MathX.Lerp( VelocityInput.x, 0, Time.Delta / 2f ), 0, MathX.Lerp( VelocityInput.z, 0, Time.Delta ) );

		Grub.Velocity = Vector3.Zero;

		if ( Input.Down( InputAction.Fire ) )
		{
			if ( Grub.Controller.IsGrounded )
			{
				Grub.Controller.GetMechanic<SquirmMechanic>().ClearGroundEntity();
			}

			VelocityInput = Vector3.Lerp( VelocityInput, new Vector3( -Grub.Player.MoveInput * ThrustSpeed, 0, ThrustSpeed / 2f ), Time.Delta );
		}
		else if ( !Grub.Controller.IsGrounded )
		{
			Grub.Position -= Vector3.Up * 300 * Time.Delta;
			Grub.Position += new Vector3( 0, 0, VelocityInput.z * Time.Delta * ThrustSpeed );
		}
		else
		{
			VelocityInput = Vector3.Zero;
		}

		if ( Grub.MoveInput == -1 )
		{
			Grub.Rotation = new Angles( 2.5f * VelocityInput.x, 0f, 0 ).ToRotation();
		}
		else if ( Grub.MoveInput == 1 )
		{
			Grub.Rotation = new Angles( -2.5f * VelocityInput.x, 180f, 0 ).ToRotation();
		}
		else
		{
			Grub.Rotation = new Angles( -2.5f * VelocityInput.x, Grub.Rotation.Angles().yaw, 0 ).ToRotation();
		}


		Grub.Position += VelocityInput * Time.Delta * ThrustSpeed;


		if ( Weapon.CurrentUses <= 0 )
		{
			FireFinished();
		}
	}

	public override void OnHolster()
	{
		base.OnHolster();
		Grub.SetAnimParameter( "jetpack_active", false );
		VelocityInput = Vector3.Zero;
	}

	public override void FireInstant()
	{

	}

	public override void FireFinished()
	{
		base.FireFinished();
	}
}
