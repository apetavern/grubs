using Grubs.Player;

namespace Grubs.UI.World;
public class AimReticle : WorldPanel
{
	public Grub Grub { get; set; }
	public Type grubsWeapon { get; set; }

	public AimReticle( Grub grub )
	{
		// This line should make it visible and not clip into the ground, some issues with it currently (s&box issue) 
		SceneObject.Flags.ViewModelLayer = true;

		Grub = grub;
		StyleSheet.Load( "UI/Stylesheets/Common.scss" );
		Add.Image( "materials/reticle/reticle.png" );
		Rotation = Rotation.RotateAroundAxis( Vector3.Up, 90f );
		grubsWeapon = Grub.ActiveChild.GetType();
	}


	public override void Tick()
	{
		base.Tick();

		Position = Grub.EyePosition + Grub.EyeRotation.Forward * 80f;
		Rotation = Rotation.RotateAroundAxis( Vector3.Forward, 0.25f );

		GrubChecks();
		if ( Grub.ActiveChild is null || Grub.ActiveChild.GetType() != grubsWeapon )
			Delete( true );
	}

	private void GrubChecks()
	{
		if ( !Grub.Controller.IsGrounded || !Grub.Controller.Velocity.IsNearlyZero( 2.5f ) )
		{
			AddClass( "Disabled" );
		}
		else if ( HasClass( "Disabled" ) )
		{
			RemoveClass( "Disabled" );
		}
	}
}
