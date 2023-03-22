namespace Grubs;

public class GrubsCamera : EntityComponent
{
	public readonly FloatRange DistanceRange = new( 128f, 2048f );
	public float Distance { get; set; } = 1024;
	public float DistanceScrollRate { get; set; } = 32f;

	private float LerpSpeed { get; set; } = 5f;
	private bool CenterOnPawn { get; set; } = true;
	private Vector3 Center { get; set; }
	private float CameraUpOffset { get; set; } = 32f;
	private TimeSince TimeSinceMousePan { get; set; }
	private static int SecondsBeforeReturnFromPan => 3;
	private Entity Target { get; set; }
	private Entity LastTarget { get; set; }
	private TimeSince TimeSinceTargetChanged { get; set; }
	private TimeUntil TimeUntilCameraUnlocked { get; set; }

	public void FrameSimulate( IClient _ )
	{
		Distance -= Input.MouseWheel * DistanceScrollRate;
		Distance = DistanceRange.Clamp( Distance );

		if ( GamemodeSystem.Instance is FreeForAll ffa )
		{
			if ( !ffa.Started )
			{
				var pregameCameraCenter = Vector3.Zero - Vector3.Up * GrubsConfig.TerrainHeight / 2f;

				var pregameTargetPosition = pregameCameraCenter + Vector3.Right * Distance;
				var pregameCurrentPosition = Camera.Position;
				Camera.Position = pregameCurrentPosition.LerpTo( pregameTargetPosition, Time.Delta * LerpSpeed );

				var pregameLookDir = (pregameCameraCenter - pregameTargetPosition).Normal;
				Camera.Rotation = Rotation.LookAt( pregameLookDir, Vector3.Up );
			}
		}

		if ( TimeUntilCameraUnlocked <= 0 )
			FindTarget();

		if ( !Target.IsValid() )
			return;

		// Get the center position, plus move the camera up a little bit.
		var cameraCenter = CenterOnPawn ? Target.Position : Center;
		cameraCenter += Vector3.Up * CameraUpOffset;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		var currentPosition = Camera.Position;
		Camera.Position = currentPosition.LerpTo( targetPosition, Time.Delta * LerpSpeed );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Camera.Rotation = Rotation.LookAt( lookDir, Vector3.Up );

		// Handle camera panning
		if ( Input.Down( InputButton.SecondaryAttack ) )
			MoveCamera();

		// Check the last time we panned the camera, update CenterOnPawn if greater than N.
		if ( !Input.Down( InputButton.SecondaryAttack ) && TimeSinceMousePan > SecondsBeforeReturnFromPan )
			CenterOnPawn = true;
	}

	public void SetTarget( Entity entity, float duration = 0 )
	{
		if ( entity == Target )
			return;

		if ( duration > 0 )
		{
			TimeUntilCameraUnlocked = duration;
		}

		LastTarget = Target;
		Target = entity;
		TimeSinceTargetChanged = 0f;
	}

	private void FindTarget()
	{
		if ( GamemodeSystem.Instance is not Gamemode gm )
			return;

		if ( gm.CameraTarget is not null )
		{
			SetTarget( gm.CameraTarget, 1f );
			return;
		}

		if ( gm.TurnIsChanging )
		{
			foreach ( var grub in Entity.All.OfType<Grub>() )
			{
				if ( grub.LifeState != LifeState.Dying )
					continue;

				if ( grub.LifeState is LifeState.Dead )
					continue;

				SetTarget( grub );
			}

			return;
		}

		foreach ( var gadget in Entity.All.OfType<Gadget>() )
		{
			if ( gadget.ShouldCameraFollow )
			{
				SetTarget( gadget );
				return;
			}
		}

		SetTarget( gm?.ActivePlayer?.ActiveGrub );
		return;
	}

	private void MoveCamera()
	{
		var delta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;
		TimeSinceMousePan = 0;
		if ( CenterOnPawn )
		{
			Center = Target.Position;

			// Check if we've moved the camera, don't center on the pawn if we have
			if ( !delta.LengthSquared.AlmostEqual( 0, 0.1f ) )
				CenterOnPawn = false;
		}

		Center += delta;
	}
}
