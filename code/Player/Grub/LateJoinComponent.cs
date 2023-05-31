namespace Grubs;

public partial class LateJoinComponent : EntityComponent<Grub>
{
	private AnimatedEntity? _parachute { get; set; }
	public bool IsDoneParachuting { get; private set; }

	protected override void OnActivate()
	{
		_parachute = new AnimatedEntity()
		{
			Model = Model.Load( "models/tools/parachute/parachute.vmdl" ),
		};

		_parachute?.SetParent( Entity, true );
		_parachute?.SetAnimParameter( "landed", false );
		_parachute?.SetAnimParameter( "deploy", true );
	}

	[GameEvent.Tick]
	void Tick()
	{
		IsDoneParachuting = Entity.Controller.IsGrounded;

		if ( !IsDoneParachuting )
		{
			if ( Game.IsServer )
				Entity.Velocity = new Vector3( GamemodeSystem.Instance.ActiveWindForce, Entity.Velocity.y, Entity.Velocity.ClampLength( 200f ).z );
		}
		else
		{
			_parachute?.SetAnimParameter( "deploy", false );
			_parachute?.SetAnimParameter( "landed", true );
			_parachute?.Delete();
		}
	}
}
