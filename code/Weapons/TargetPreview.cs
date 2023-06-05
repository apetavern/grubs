namespace Grubs;

public partial class TargetPreview : ModelEntity
{
	public bool IsTargetSet { get; private set; } = false;

	private Grub _grub => Owner as Grub;
	private Weapon _weapon => _grub.ActiveWeapon;

	public override void Spawn()
	{
		SetModel( "models/weapons/targetindicator/targetindicator.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( Tag.Preview );
	}

	public virtual void Hide()
	{
		if ( Game.IsServer )
			Delete();
	}

	public override void Simulate( IClient client )
	{
		EnableDrawing = IsTargetSet || _grub.Controller.ShouldShowWeapon() && _weapon.HasChargesRemaining;

		if ( !IsTargetSet )
		{
			Position = _grub.Player.MousePosition.WithY( -33 );
			Rotation = Rotation.Lerp( Rotation, Rotation.RotateAroundAxis( Vector3.Right, 200 ), Time.Delta );
		}

		_grub.Player.GrubsCamera.AutomaticRefocus = !_weapon.HasChargesRemaining;
		UI.Cursor.Enabled( "Weapon", _weapon.FiringType == FiringType.Cursor );

		if ( Game.IsClient )
		{
			var fireHint = _weapon.FiringType == FiringType.Charged ? "Fire (Hold)" : "Fire";
			_weapon.InputHintWorldPanel?.UpdateInput( InputAction.Fire, !IsTargetSet ? "Mark" : fireHint );
		}
	}

	public virtual void LockCursor() => IsTargetSet = true;
	public virtual void UnlockCursor() => IsTargetSet = false;
}
