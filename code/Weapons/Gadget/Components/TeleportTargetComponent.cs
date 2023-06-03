namespace Grubs;

[Prefab]
public partial class TeleportTargetComponent : WeaponComponent
{
	/// <summary>
	/// Teleport a target.
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="position">(Optional) leave null for a random position.</param>
	public void Teleport( Entity entity, Vector3? position = null )
	{
		var teleportPos = position is null ? GamemodeSystem.Instance.Terrain.FindSpawnLocation( traceDown: true, size: 32f ) : position.Value;
		if ( entity is Gadget )
			teleportPos = Grub.EyePosition + Vector3.Up * 10f;

		Particles.Create( "particles/teleport/teleport_up.vpcf", entity.Position );
		entity.Position = teleportPos;
		Particles.Create( "particles/teleport/teleport_down.vpcf", entity.Position );
	}
}
