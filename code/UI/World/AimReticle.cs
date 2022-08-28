using Grubs.Player;

namespace Grubs.UI.World;
public class AimReticle : WorldPanel
{
	public Worm Worm { get; set; }
	public Type grubsWeapon { get; set; }

	public AimReticle( Worm worm )
	{
		// This line should make it visible and not clip into the ground, some issues with it currently (s&box issue) 
		SceneObject.Flags.ViewModelLayer = true;

		Worm = worm;
		StyleSheet.Load( "UI/Stylesheets/Common.scss" );
		Add.Image( "materials/reticle/reticle.png" );
		Rotation = Rotation.RotateAroundAxis( Vector3.Up, 90f );
		grubsWeapon = Worm.ActiveChild.GetType();
	}


	public override void Tick()
	{
		base.Tick();

		Position = Worm.EyePosition + Worm.EyeRotation.Forward * 80f;
		Rotation = Rotation.RotateAroundAxis( Vector3.Forward, 0.25f );

		WormChecks();
		if ( Worm.ActiveChild is null || Worm.ActiveChild.GetType() != grubsWeapon )
		{
			DestroyReticle();
		}
	}

	private void WormChecks()
	{

		if ( !Worm.Controller.IsGrounded || !Worm.Controller.Velocity.IsNearlyZero( 2.5f ) )
		{
			AddClass( "Disabled" );
		}
		else if ( HasClass( "Disabled" ) )
		{
			RemoveClass( "Disabled" );
		}

	}

	public void DestroyReticle()
	{
		Worm.LastActiveChild.HasReticle = false;
		Delete( true );
	}
}
