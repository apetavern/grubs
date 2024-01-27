using Grubs.Common;
using Grubs.Equipment;
using Grubs.Player.Controller;

namespace Grubs.Player;

[Title( "Grubs - Container" )]
[Category( "Grubs" )]
public sealed class Grub : Component
{
	[Property] public required HealthComponent Health { get; set; }
	[Property] public EquipmentComponent? ActiveEquipment { get; set; }

	private EquipmentComponent? LastEquipped { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";

	protected override void OnStart()
	{
		base.OnStart();

		ActiveEquipment?.Equip( Components.Get<SkinnedModelRenderer>() );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Input.Pressed( "toggle_equipment" ) )
			return;

		if ( ActiveEquipment is null )
		{
			var controller = Components.Get<GrubPlayerController>();
			if ( controller is null )
				return;

			controller.LookAngles = Rotation.FromPitch( 0f ).Angles();

			ActiveEquipment = LastEquipped;
			ActiveEquipment?.Equip( Components.Get<SkinnedModelRenderer>() );
		}
		else
		{
			ActiveEquipment?.Holster();
			LastEquipped = ActiveEquipment;
			ActiveEquipment = null;
		}
	}
}
