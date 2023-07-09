namespace Grubs;

[Prefab]
public partial class ParachuteComponent : WeaponComponent
{
	[Net, Predicted]
	private bool Deployed { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string DeploySound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string DisengageSound { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		Weapon.SetAnimParameter( "deploy", Deployed );
		Weapon.SetAnimParameter( "landed", !Deployed );

		if ( IsFiring )
			Fire();

		if ( Deployed )
		{
			var chuteHelper = new GrubParachuteHelper
			{
				FallSpeed = 70f,
				IsAffectedByWind = true,
				IsPlayerControlled = true,
			};

			chuteHelper.Simulate( Grub );

			if ( Grub.Controller.IsGrounded )
				FireFinished();
		}
	}

	public override void FireInstant()
	{
		Deploy();
		base.FireInstant();
	}

	public override void FireFinished()
	{
		Disengage();
		base.FireFinished();
	}

	public override void OnHolster()
	{
		Disengage();
		base.OnHolster();
	}

	private void Deploy()
	{
		if ( !Deployed )
			Weapon.PlaySound( DeploySound );

		Deployed = true;
	}

	private void Disengage()
	{
		if ( Deployed )
			Weapon.PlaySound( DisengageSound );

		Deployed = false;
	}
}
