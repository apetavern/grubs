using Grubs.Common;
using Grubs.Equipment;
using Grubs.Player.Controller;

namespace Grubs.Player;

[Title( "Grubs - Container" ), Category( "Grubs" )]
public sealed class Grub : Component
{
	[Property] public required HealthComponent Health { get; set; }
	[Property] public required GrubPlayerController PlayerController { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }
	[Property, ReadOnly] public EquipmentComponent? ActiveEquipment { get; set; }

	[Property] public required GameObject BazookaPrefab { get; set; }
	[Property, ReadOnly] private GameObject? Bazooka { get; set; }

	private EquipmentComponent? LastEquipped { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";

	protected override void OnStart()
	{
		base.OnStart();

		if ( !IsProxy )
			InitializeLocal();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Input.Pressed( "toggle_equipment" ) && !IsProxy )
		{
			if ( ActiveEquipment is null )
			{
				var controller = Components.Get<GrubPlayerController>();
				if ( controller is null )
					return;

				controller.LookAngles = Rotation.FromPitch( 0f ).Angles();

				if ( LastEquipped is null )
				{
					Bazooka = BazookaPrefab.Clone();
					AssignEquipment();
				}
				else
					ActiveEquipment = LastEquipped;

				DeployEquipment();
			}
			else
			{
				HolsterEquipment();
			}
		}
	}

	[Broadcast]
	private void AssignEquipment()
	{
		if ( Bazooka is not null )
			ActiveEquipment = Bazooka.Components.Get<EquipmentComponent>();
	}

	[Broadcast]
	private void DeployEquipment()
	{
		ActiveEquipment?.Deploy( this );
	}

	[Broadcast]
	private void HolsterEquipment()
	{
		ActiveEquipment?.Holster();
		LastEquipped = ActiveEquipment;
		ActiveEquipment = null;
	}

	private void InitializeLocal()
	{
		if ( GrubFollowCamera.Local is not null )
			GrubFollowCamera.Local.Target = GameObject;
	}

	[ConCmd( "gr_take_dmg" )]
	public static void TakeDmgCmd( float hp )
	{
		var grub = GameManager.ActiveScene.GetAllComponents<Grub>().FirstOrDefault();
		if ( grub is null )
			return;

		grub.Health.TakeDamage( hp );
	}
}
