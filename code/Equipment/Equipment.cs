using Grubs.Player;

namespace Grubs.Equipment;

public class EquipmentComponent : Component
{
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;

	public Grub? Grub { get; set; }

	protected override void OnUpdate()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if ( Grub is null )
			return;

		var show = Grub.PlayerController.ShouldShowWeapon();
		Model.Enabled = show;
	}

	public void Deploy( Grub grub )
	{
		Grub = grub;

		var target = grub.Components.Get<SkinnedModelRenderer>();
		Model.BoneMergeTarget = target;
		Model.Enabled = true;
	}

	public void Holster()
	{
		Model.BoneMergeTarget = null;
		Model.Enabled = false;
	}
}
