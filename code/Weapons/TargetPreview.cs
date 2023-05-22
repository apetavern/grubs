using Sandbox;

namespace Grubs;

public partial class TargetPreview : ModelEntity
{
	private Grub _grub => Owner as Grub;
	private Weapon _weapon => _grub.ActiveWeapon;

	private bool _isTargetSet;

	public virtual void Display( Grub grub )
	{
		if ( Game.IsServer )
		{
			SetModel( "models/weapons/targetindicator/targetindicator.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Tags.Add( Tag.Preview );
		}

		Owner = grub;
		_isTargetSet = false;
	}

	public virtual void Hide()
	{
		if ( Game.IsServer )
			Delete();
	}

	public override void Simulate( IClient client )
	{
		EnableDrawing = _isTargetSet || _grub.Controller.ShouldShowWeapon() && _weapon.HasChargesRemaining;

		if ( !_isTargetSet )
		{
			Position = _grub.Player.MousePosition.WithY( -33 );
			Rotation = Rotation.Lerp( Rotation, Rotation.RotateAroundAxis( Vector3.Right, 200 ), Time.Delta );
		}

		_grub.Player.GrubsCamera.AutomaticRefocus = !_weapon.HasChargesRemaining;
		UI.Cursor.Enabled( "Weapon", _weapon.FiringType == FiringType.Cursor );
	}

	public virtual void LockCursor() => _isTargetSet = true;
	public virtual void UnlockCursor() => _isTargetSet = false;
}
