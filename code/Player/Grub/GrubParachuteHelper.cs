namespace Grubs;

public struct GrubParachuteHelper
{
	public float Drag { get; set; }
	public bool IsAffectedByWind { get; set; }
	public bool IsPlayerControlled { get; set; }

	public void Fall( Grub grub )
	{
		var wind = IsAffectedByWind ? GamemodeSystem.Instance.ActiveWindForce : 0;
		var playerInput = IsPlayerControlled ? grub.Player.MoveInput : 0;
		grub.Velocity = new Vector3( grub.Velocity.x - playerInput + wind, grub.Velocity.y, grub.Velocity.ClampLength( Drag ).z );
	}
}
