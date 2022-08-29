using System.Threading.Tasks;
using Grubs.Player;

namespace Grubs.UI;

public class DamageNumber : WorldPanel
{
	private const float RiseSpeed = 5;
	private static Vector3 Offset => Vector3.Up * 64;

	public Grub Grub { get; }

	public DamageNumber( Grub grub, float damage )
	{
		Grub = grub;

		StyleSheet.Load( "/UI/Stylesheets/DamageNumber.scss" );

		Add.Label( $"-{Math.Floor( damage )}", "grub-damage" );

		const float width = 600;
		const float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;

		Position = grub.Position + Offset;
		_ = DelayDelete();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Grub is null || !Grub.IsValid )
		{
			Delete();
			return;
		}

		Position += Vector3.Up * RiseSpeed * Time.Delta;
		Rotation = Rotation.LookAt( Vector3.Right );

		if ( Local.Pawn is ISpectator { Camera: GrubsCamera camera } )
			WorldScale = 1.5f + (camera.Distance / camera.MaxDistance * 2);
	}

	private async Task DelayDelete()
	{
		await GameTask.Delay( 5000 );
		Delete();
	}
}
