using Grubs.Player;

namespace Grubs.Equipment;

[Title( "Grubs - Equipment" ), Category( "Equipment" )]
public class EquipmentComponent : Component
{
	[Property] public required string Name { get; set; } = "";
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;
	[Property, ResourceType( "jpg" )] public string Icon { get; set; } = "";
	[Property, Sync] public int SlotIndex { get; set; }

	public bool Deployed { get; set; }

	public Grub? Grub { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Holster();
	}

	protected override void OnUpdate()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if ( Grub is null )
			return;

		var show = Grub.PlayerController.ShouldShowWeapon() && Deployed;
		Model.Enabled = show;
	}

	public void Deploy( Grub grub )
	{
		Grub = grub;

		var target = grub.GameObject.GetAllObjects( true ).First( c => c.Name == "hold_L" );
		GameObject.SetParent( grub.GameObject, false );
		Model.BoneMergeTarget = grub.Components.Get<SkinnedModelRenderer>();
		Model.Enabled = true;
		Deployed = true;
	}

	public void Holster()
	{
		Model.BoneMergeTarget = null;
		Model.Enabled = false;
		Deployed = false;
	}
}
