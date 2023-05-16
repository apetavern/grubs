namespace Grubs;

[Prefab]
public partial class HomingMissileComponent : WeaponComponent
{
	[Prefab, ResourceType( "sound" )]
	public string UseSound { get; set; }

	[Net]
	public AnimatedEntity TargetPreview { get; set; }

	public override void OnDeploy()
	{
		if ( !Game.IsServer )
			return;

		TargetPreview = new AnimatedEntity( "models/weapons/targetindicator/targetindicator.vmdl" );
		TargetPreview.Tags.Add( "preview" );
		TargetPreview.Owner = Grub;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( Game.IsServer )
		{
			if ( TargetPreview.IsValid() )
				TargetPreview.Delete();
		}
		else
		{
			Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( !TargetPreview.IsValid() )
			return;

		Grub.Player.GrubsCamera.AutomaticRefocus = !Weapon.HasChargesRemaining;

		if ( Game.IsServer )
		{
			if ( IsFiring )
				Fire();
			else
				IsFiring = false;
		}
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( UseSound );

		FireFinished();

		Grub.Player.Inventory.SetActiveWeapon( Weapon.FromPrefab( "prefabs/weapons/bazooka/bazooka.prefab" ), true );
		Grub.ActiveWeapon.Target = Grub.Player.MousePosition;
	}
}
