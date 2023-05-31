namespace Grubs;

public partial class LateJoinComponent : EntityComponent<Grub>
{
	private AnimatedEntity? _parachute { get; set; }

	protected override void OnActivate()
	{
		_parachute = new AnimatedEntity()
		{
			Model = Model.Load( "models/tools/parachute/parachute.vmdl" ),
		};

		_parachute.SetParent( Entity, true );
		_parachute.SetAnimParameter( "landed", false );
		_parachute.SetAnimParameter( "deploy", true );
	}

	[GameEvent.Tick]
	void Tick()
	{
		if ( Game.IsServer )
			Entity.Velocity = new Vector3( GamemodeSystem.Instance.ActiveWindForce, Entity.Velocity.y, Entity.Velocity.ClampLength( 70f ).z );

		if ( Entity.Controller.IsGrounded && _parachute is not null )
		{
			_parachute.SetAnimParameter( "deploy", false );
			_parachute.SetAnimParameter( "landed", true );
			_parachute.Delete();
		}
	}
}
