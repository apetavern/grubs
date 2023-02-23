namespace Grubs;

public class AirMoveMechanic : ControllerMechanic
{
	public static float AirControl => 4.0f;
	public static float AirAcceleration => 8.0f;
	public static float FallVelocityDamageThreshold => MathF.Sqrt( 2 * Gravity * 15 * 12 );
	public static float FallVelocityFatalThreshold => MathF.Sqrt( 2 * Gravity * 50 * 12 );
	public static float FallPunchThreshold => 300f;
	public static float FallDamage => 100f / (FallVelocityFatalThreshold - FallVelocityDamageThreshold);
	public static float FallDamageModifier => 0.15f;

	public bool IsHardFalling { get; set; } = false;
	public float FallVelocity { get; set; }

	protected override bool ShouldStart()
	{
		return true;
	}

	protected override void Simulate()
	{
		var ctrl = Controller;

		FallVelocity = -ctrl.Velocity.z;

		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;
		ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );

		var groundedAtStart = ctrl.GroundEntity.IsValid();

		CheckFallDamage();

		if ( groundedAtStart )
			return;

		var wishVel = ctrl.GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;

		ctrl.Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
		ctrl.Velocity += ctrl.BaseVelocity;
		ctrl.Move();
		ctrl.Velocity -= ctrl.BaseVelocity;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

	}

	private void CheckFallDamage()
	{
		IsHardFalling = GroundEntity is null && FallVelocity > FallVelocityDamageThreshold && Grub.IsTurn;

		if ( Grub.LifeState is LifeState.Dead || FallVelocity < FallPunchThreshold || Grub.GetWaterLevel() >= 1f )
			return;

		if ( FallVelocity > FallVelocityDamageThreshold )
			ApplyFallDamage();
	}

	private void ApplyFallDamage()
	{
		if ( Game.IsServer && Grub.IsTurn )
			GamemodeSystem.Instance.UseTurn();

		float fallDamage = (FallVelocity - FallVelocityDamageThreshold) * FallDamage * FallDamageModifier;
		if ( fallDamage < 1 )
		{
			fallDamage = 1;
		}
		Grub.TakeDamage( DamageInfoExtension.FromFall( fallDamage, Grub ) );
	}
}
