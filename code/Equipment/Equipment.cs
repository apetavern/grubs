namespace Grubs.Equipment;

public class EquipmentComponent : Component
{
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;

	public void Equip( SkinnedModelRenderer target )
	{
		Model.BoneMergeTarget = target;
		Model.Enabled = true;
	}

	public void Holster()
	{
		Model.BoneMergeTarget = null;
		Model.Enabled = false;
	}
}
