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

		if ( Deployed )
		{
			var chuteHelper = new GrubParachuteHelper
			{
				FallSpeed = 70f,
				IsAffectedByWind = true,
				IsPlayerControlled = true,
			};

			chuteHelper.Simulate( Grub );

			if ( IsFiring || Grub.Controller.IsGrounded )
				FireFinished();
		}

		if ( IsFiring )
			Fire();
	}

	public override void FireInstant()
	{
		IsFiring = false;

		if ( !Grub.Controller.IsGrounded )
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
