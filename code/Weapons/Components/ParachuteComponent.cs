namespace Grubs;

[Prefab]
public partial class ParachuteComponent : WeaponComponent
{
	[Net, Predicted]
	private bool Deployed { get; set; } = false;

	[Prefab, ResourceType( "sound" )]
	public string DeploySound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string DisengageSound { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

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

			chuteHelper.Fall( Grub );
		}

		if ( Grub.Controller.IsGrounded && Deployed )
			FireFinished();
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
			Weapon.PlayScreenSound( DeploySound );

		Deployed = true;
		Weapon.SetAnimParameter( "landed", false );
		Weapon.SetAnimParameter( "deploy", true );
	}

	private void Disengage()
	{
		if ( Deployed )
			Weapon.PlayScreenSound( DisengageSound );

		Deployed = false;
		Weapon.SetAnimParameter( "deploy", false );
		Weapon.SetAnimParameter( "landed", true );
	}
}
