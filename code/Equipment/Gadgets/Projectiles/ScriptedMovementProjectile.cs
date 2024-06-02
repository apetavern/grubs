namespace Grubs.Equipment.Gadgets.Projectiles;

[Title( "Grubs - Scripted Movement Projectile" ), Category( "Equipment" )]
public class ScriptedMovementProjectile : Projectile
{
	[Property] public Vector3 Movement { get; set; }

	[Property] public Vector3 OverrideMovement { get; set; } // Set externally if we need to change the movement temporarily

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( OverrideMovement != Vector3.Zero )
		{
			Transform.Position += OverrideMovement * Time.Delta;
			return;
		}

		Transform.Position += Movement * Time.Delta;
	}
}
