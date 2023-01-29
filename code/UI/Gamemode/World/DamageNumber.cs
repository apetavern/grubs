using System.Threading.Tasks;
using Grubs.Player;
using Grubs.States;

namespace Grubs.UI;

public sealed class DamageNumber : WorldPanel
{
	private const float RiseSpeed = 5;
	private static Vector3 Offset => Vector3.Up * 64;

	public Grub Grub { get; }

	public DamageNumber( Grub grub, float damage )
	{
		Grub = grub;

		StyleSheet.Load( "/UI/Stylesheets/DamageNumber.scss" );

		if ( damage < 0 )
			Add.Label( $"+{Math.Floor( Math.Abs( damage ) )}", "grub-heal" );
		else
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

		if ( !Grub.IsValid )
		{
			Delete();
			return;
		}

		Position += Vector3.Up * RiseSpeed * Time.Delta;
		Rotation = Rotation.LookAt( Vector3.Right );

		if ( BaseGamemode.Instance is not null )
			WorldScale = (1.0f + BaseGamemode.Instance.DistanceRange.LerpInverse( -Camera.Position.y )) * 3f;
	}

	private async Task DelayDelete()
	{
		await GameTask.Delay( 5000 );
		Delete();
	}
}
