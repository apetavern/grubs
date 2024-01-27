namespace Grubs.Player;

public class GrubFollowCamera : Component
{
	[Property] public required GameObject Target { get; set; }

	public float Distance { get; set; } = 1024f;

	protected override void OnUpdate()
	{
		var cam = GameObject;
		var targetPos = Target.Transform.Position + Vector3.Right * Distance;
		targetPos.z += 32f;
		cam.Transform.Position = cam.Transform.Position.LerpTo( targetPos, Time.Delta * 5f );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Distance -= Input.MouseWheel.y * 32f;
		Distance = Distance.Clamp( 128f, 2048f );

		AdjustHighlightOutline();
	}

	private void AdjustHighlightOutline()
	{
		if ( !Target.Components.TryGet( out HighlightOutline highlight ) )
			return;

		var desiredWidth = (1 / Distance * 100).Clamp( 0f, 0.35f );
		highlight.Width = highlight.Width.LerpTo( desiredWidth, Time.Delta * 5f );
	}
}
