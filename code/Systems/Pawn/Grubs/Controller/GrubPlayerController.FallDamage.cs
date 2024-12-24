using Grubs.Common;

namespace Grubs.Systems.Pawn.Grubs.Controller;

public partial class GrubPlayerController
{
	public static float FallVelocityDamageThreshold => MathF.Sqrt( 2 * 800f * 15 * 12 );
	public static float FallVelocityFatalThreshold => MathF.Sqrt( 2 * 800f * 50 * 12 );
	public static float FallDistanceThreshold => 50f;
	public static float FallPunchThreshold => 300f;
	public static float FallDamage => 100f / (FallVelocityFatalThreshold - FallVelocityDamageThreshold);
	public static float FallDamageModifier => 0.13f;

	public float LastGroundHeight { get; set; }

	[Sync] public bool IsHardFalling { get; set; }
	public float FallVelocity { get; set; }

	private void UpdateFallVelocity()
	{
		FallVelocity = -Velocity.z;
		IsHardFalling = !IsGrounded && FallVelocity > FallVelocityDamageThreshold;
	}

	public void CheckFallDamage()
	{
		if ( FallVelocity < FallPunchThreshold )
			return;

		if ( LastGroundHeight - WorldPosition.z < FallDistanceThreshold )
			return;

		if ( FallVelocity > FallVelocityDamageThreshold )
			return;
		// ApplyFallDamage();
	}

	// private void ApplyFallDamage()
	// {
	// 	var fallDamage = (FallVelocity - FallVelocityDamageThreshold) * FallDamage * FallDamageModifier;
	// 	var health = Sandbox.GameObject.Components.Get<Health>();
	// 	health?.TakeDamage( GrubsDamageInfo.FromFall( fallDamage, global::Grubs.Pawn.Grub.Id, global::Grubs.Pawn.Grub.Name ) );
	// }
}
