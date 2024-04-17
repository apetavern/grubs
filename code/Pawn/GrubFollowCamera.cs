using Grubs.Gamemodes;

namespace Grubs.Pawn;

public class GrubFollowCamera : Component
{
	[Property, ReadOnly] public GameObject? Target { get; set; }
	public static GrubFollowCamera? Local { get; set; }
	public float Distance { get; set; } = 1024f;

	public GrubFollowCamera()
	{
		Local = this;
	}

	protected override void OnUpdate()
	{
		var component = Scene.Directory.FindComponentByGuid( Gamemode.FFA.ActivePlayerId );
		if ( component is not Player player )
			return;

		if ( player.ActiveGrub is not null )
			Target = player.ActiveGrub.GameObject;

		if ( Target is null )
			return;

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
		var highlights = Scene.GetAllComponents<HighlightOutline>();

		foreach ( var highlight in highlights )
		{
			var desiredWidth = (1 / Distance * 100).Clamp( 0f, 0.35f );
			highlight.Width = highlight.Width.LerpTo( desiredWidth, Time.Delta * 5f );
		}
	}
}
