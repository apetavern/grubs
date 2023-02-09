namespace Grubs;

[Prefab]
public class MeleeComponent : WeaponComponent
{
	[Prefab]
	public float Damage { get; set; } = 25;

	[Prefab]
	public Vector3 HitForce { get; set; }

	[Prefab]
	public Vector3 HitSize { get; set; }

	[Prefab]
	public float HitDelay { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
	}

	public override void FireInstant()
	{
		var grubsHit = GetGrubsInSwing();
		Log.Info( grubsHit.Count );

		Grub.SetAnimParameter( "fire", true );
	}

	public override void FireCharged()
	{
		Grub.SetAnimParameter( "fire", true );
	}

	private List<Grub> GetGrubsInSwing()
	{
		// Scrap this and just use a ray trace instead
		var min = new Vector3(
			Grub.Position.x + 16f * Grub.Facing,
			-16f,
			Grub.EyePosition.z + (Grub.LookAngles.pitch * -Grub.Facing) - (HitSize.z / 2) );

		var max = new Vector3(
			min.x + HitSize.x * Grub.Facing,
			16f,
			min.z + (HitSize.z / 2) );

		var grubsHit = new List<Grub>();

		if ( Grub.Facing > 0 )
			DebugOverlay.Box( max, min, Color.Red, 10 );
		else
			DebugOverlay.Box( min, max, Color.Red, 10 );

		return grubsHit;
	}
}
