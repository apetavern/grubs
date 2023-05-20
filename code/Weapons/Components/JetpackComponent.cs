using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{

	[Prefab, Net]
	public float ThrustSpeed { get; set; } = 25f;

	[Prefab, Net]
	public float Fuel { get; set; } = 20f;

	[Net]
	public float FuelCount { get; set; }

	[Net]
	public Vector3 VelocityInput { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		Grub.SetAnimParameter( "jetpack_active", true );
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * VelocityInput.x / 15f );

		VelocityInput = new Vector3( MathX.Lerp( VelocityInput.x, 0, Time.Delta / 2f ), 0, MathX.Lerp( VelocityInput.z, 0, Time.Delta ) );

		Grub.Velocity = Vector3.Zero;

		if ( Input.Down( InputAction.Fire ) )
		{
			if ( Grub.Controller.IsGrounded )
			{
				Grub.Controller.GetMechanic<SquirmMechanic>().ClearGroundEntity();
			}

			FuelCount -= Time.Delta;

			if ( FuelCount <= 0 )
			{
				FireFinished();
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
			Grub.Rotation = Rotation.Identity;
		}
		else if ( Grub.MoveInput == 1 )
		{
			Grub.Rotation = new Angles( 0, 180f, 0 ).ToRotation();
		}
		var tr = Trace.Box( Grub.Controller.Hull, Grub.Position, Grub.Position + VelocityInput * Time.Delta * ThrustSpeed ).Ignore( Grub ).Run();

		MoveHelper mover = new MoveHelper( Grub.Position, VelocityInput * Time.Delta * ThrustSpeed );
		mover.Trace = Trace.Box( Grub.Controller.Hull, Grub.Position, Grub.Position + VelocityInput * Time.Delta * ThrustSpeed ).Ignore( Grub );
		mover.TryMove( Time.Delta );
		Grub.Position = mover.Position + mover.Velocity;
		//Grub.Position += VelocityInput * Time.Delta * ThrustSpeed;

	}

	public override void OnDeploy()
	{
		base.OnDeploy();
		FuelCount = Fuel;
	}

	public override void OnHolster()
	{
		base.OnHolster();
		Grub.SetAnimParameter( "jetpack_active", false );
		VelocityInput = Vector3.Zero;
		FuelCount = Fuel;
		Weapon.Ammo -= 1;
	}

	public override void FireFinished()
	{

		FuelCount = Fuel;
		Player.Inventory.SetActiveWeapon( null, true );
	}
}
