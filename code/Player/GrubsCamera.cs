using Grubs.Crates;
using Grubs.States;
using Grubs.Weapons.Base;

namespace Grubs.Player;

public sealed partial class GrubsCamera : CameraMode
{
	public float Distance { get; set; } = 1024;
	public float DistanceScrollRate { get; set; } = 32f;
	public float MinDistance { get; set; } = 128f;
	public float MaxDistance { get; set; } = 2048f;

	private float LerpSpeed { get; set; } = 5f;
	private bool CenterOnPawn { get; set; } = true;
	private Vector3 Center { get; set; }
	private float CameraUpOffset { get; set; } = 32f;

	private TimeSince TimeSinceMousePan { get; set; }
	private static int SecondsBeforeReturnFromPan => 3;

	private Entity? Target { get; set; }
	private Entity? LastTarget { get; set; }
	private TimeSince TimeSinceTargetChanged { get; set; }

	public override void Update()
	{
		Distance -= Input.MouseWheel * DistanceScrollRate;
		Distance = Distance.Clamp( MinDistance, MaxDistance );

		FindTarget();

		if ( Target is null || !Target.IsValid )
			return;

		// Get the center position, plus move the camera up a little bit.
		var cameraCenter = (CenterOnPawn) ? Target.Position : Center;
		cameraCenter += Vector3.Up * CameraUpOffset;

		var targetPosition = cameraCenter + Vector3.Right * Distance;
		Position = Position.LerpTo( targetPosition, Time.Delta * LerpSpeed );

		var lookDir = (cameraCenter - targetPosition).Normal;
		Rotation = Rotation.LookAt( lookDir, Vector3.Up );

		// Handle camera panning
		if ( Input.Down( InputButton.SecondaryAttack ) )
			MoveCamera();

		// Check the last time we panned the camera, update CenterOnPawn if greater than N.
		if ( !Input.Down( InputButton.SecondaryAttack ) && TimeSinceMousePan > SecondsBeforeReturnFromPan )
			CenterOnPawn = true;
	}

	private void FindTarget()
	{
		if ( GrubsGame.Current.CurrentState is not BaseGamemode gamemode )
			return;

		if ( gamemode.IsTurnChanging )
		{
			foreach ( var grub in Entity.All.OfType<Grub>() )
			{
				if ( grub.LifeState != LifeState.Dying )
					continue;

				ChangeTarget( grub );
				return;
			}

			foreach ( var crate in Entity.All.OfType<BaseCrate>() )
			{
				if ( crate.Resolved )
					continue;

				ChangeTarget( crate );
				return;
			}
		}
		else
		{
			foreach ( var projectile in Entity.All.OfType<Projectile>() )
			{
				ChangeTarget( projectile );
				return;
			}

			foreach ( var grub in Entity.All.OfType<Grub>() )
			{
				if ( !grub.HasBeenDamaged || grub.Resolved )
					continue;

				ChangeTarget( grub );
				return;
			}

			ChangeTarget( TeamManager.Instance.CurrentTeam.ActiveGrub );
			return;
		}

		ChangeTarget( LastTarget is null ? GrubsGame.Current.TerrainModel.Center : null );
	}

	private void ChangeTarget( Entity? target )
	{
		if ( Target == target )
			return;

		LastTarget = Target;
		Target = target;
		TimeSinceTargetChanged = 0;
	}

	private void MoveCamera()
	{
		var delta = new Vector3( -Mouse.Delta.x, 0, Mouse.Delta.y ) * 2;
		TimeSinceMousePan = 0;

		if ( CenterOnPawn )
		{
			Center = Target!.Position;

			// Check if we've moved the camera, don't center on the pawn if we have
			if ( !delta.LengthSquared.AlmostEqual( 0, 0.1f ) )
				CenterOnPawn = false;
		}

		Center += delta;
	}
}
