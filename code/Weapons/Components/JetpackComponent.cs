using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class JetpackComponent : WeaponComponent
{

	[Prefab, Net]
	public float ThrustSpeed { get; set; } = 25f;

	[Prefab, Net]
	public float Fuel { get; set; } = 10f;

	[Net]
	public float FuelCount { get; set; }

	[Net]
	public Vector3 VelocityInput { get; set; }


	Particles jetparticle1 { get; set; }


	Particles jetparticle2 { get; set; }


	Particles jetparticle3 { get; set; }

	//particles/blueflame/blueflame_base.vpcf

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		Grub.SetAnimParameter( "jetpack_active", true );
		Grub.SetAnimParameter( "jetpack_dir", Grub.Facing * VelocityInput.x / 15f );

		VelocityInput = new Vector3( MathX.Lerp( VelocityInput.x, 0, Time.Delta / 2f ), 0, MathX.Lerp( VelocityInput.z, 0, Time.Delta ) );

		Grub.Velocity = Vector3.Zero;

		if ( Grub.Player.LookInput > 0 )
		{
			if ( Grub.Controller.IsGrounded )
			{
				Grub.Controller.GetMechanic<SquirmMechanic>().ClearGroundEntity();
			}

			if ( jetparticle1 is null )
			{
				jetparticle1 = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
				jetparticle2 = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );
				jetparticle3 = Particles.Create( "particles/blueflame/blueflame_continuous.vpcf" );

				jetparticle1.SetEntityAttachment( 0, Weapon, "jet_middle" );
				jetparticle2.SetEntityAttachment( 0, Weapon, "jet_left" );
				jetparticle3.SetEntityAttachment( 0, Weapon, "jet_right" );
			}
			else
			{
				jetparticle1.SetEntityAttachment( 0, Weapon, "jet_middle" );
				jetparticle2.SetEntityAttachment( 0, Weapon, "jet_left" );
				jetparticle3.SetEntityAttachment( 0, Weapon, "jet_right" );
			}

			if ( jetparticle1 is not null )
			{
				if ( Grub.Player.MoveInput != 0 )
				{
					jetparticle1.EnableDrawing = true;
				}
				else
				{
					jetparticle1.EnableDrawing = false;
				}
				jetparticle2.EnableDrawing = true;
				jetparticle3.EnableDrawing = true;
			}

			Sound.FromScreen( "torch_fire" ).SetPitch( 1f ).SetVolume( 0.5f );

			FuelCount -= Time.Delta;

			if ( FuelCount <= 0 )
			{
				FireFinished();
			}

			VelocityInput = Vector3.Lerp( VelocityInput, new Vector3( -Grub.Player.MoveInput * ThrustSpeed, 0, ThrustSpeed / 2f ), Time.Delta );
		}
		else if ( !Grub.Controller.IsGrounded )
		{
			if ( jetparticle1 is not null )
			{
				jetparticle1.EnableDrawing = false;
				jetparticle2.EnableDrawing = false;
				jetparticle3.EnableDrawing = false;
			}
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

		MoveHelper mover = new MoveHelper( Grub.Position, VelocityInput * Time.Delta * ThrustSpeed );
		mover.Trace = Trace.Box( Grub.Controller.Hull, Grub.Position, Grub.Position + VelocityInput * Time.Delta * ThrustSpeed ).Ignore( Grub );
		mover.TryMove( Time.Delta );
		Grub.Position = mover.Position + mover.Velocity;
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
