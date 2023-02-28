namespace Grubs;

public class DamageNumber : WorldPanel
{
	private const float RiseSpeed = 5;
	private static Vector3 Offset => Vector3.Up * 64;

	public Grub Grub { get; }

	public DamageNumber( Grub grub, float damage )
	{
		Grub = grub;

		StyleSheet.Load( "/UI/World/DamageNumber.scss" );

		if ( damage < 0 )
			Add.Label( $"+{Math.Floor( Math.Abs( damage ) )}", "grub-heal" );
		else
			Add.Label( $"-{Math.Floor( damage )}", "grub-damage" );

		const float width = 600;
		const float height = 300;

		PanelBounds = new Rect( -width / 2, -height / 2, width, height );

		SceneObject.Flags.BloomLayer = false;

		Position = grub.Position + Offset;
		Position = Position.WithY( -34 );
		_ = DelayDelete();
	}

	public override void Tick()
	{
		if ( Game.LocalPawn is not Player player || !Grub.IsValid() )
		{
			Delete();
			return;
		}

		Position += Vector3.Up * RiseSpeed * Time.Delta;
		Rotation = Rotation.LookAt( Vector3.Right );

		WorldScale = (1.0f + player.GrubsCamera.DistanceRange.LerpInverse( Camera.Position.y )) * 3f;
	}

	private async Task DelayDelete()
	{
		await GameTask.Delay( 5000 );
		Delete();
	}
}
