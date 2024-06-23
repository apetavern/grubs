using Grubs.Common;
using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Equipment.Gadgets.Ground;

[Title( "Grubs - Proximity Explosive" ), Category( "Equipment" )]

public partial class ProximityExplosive : Component, Component.ITriggerListener, IResolvable
{
	[Property] public float Radius { get; set; } = 64.0f;
	[Property] public float Damage { get; set; } = 100.0f;
	[Property] public float ArmTime { get; set; } = 5.0f;
	[Property] public float DetonateTime { get; set; } = 5.0f;
	[Property] public bool DetonateOnDeath { get; set; } = false;
	[Property, ResourceType( "sound" )] public string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] public ParticleSystem Particles { get; set; }

	[Property] public bool IsDud { get; set; }
	public bool IsArmed { get; set; }
	public bool IsDetonating { get; set; }
	private TimeSince _createdAt { get; set; }
	private TimeSince _detonatedAt { get; set; }
	private List<Grub> _grubs { get; set; } = new();

	public bool Resolved => !IsDetonating || !IsArmed;

	public virtual void OnArm() { }
	public virtual void OnTrigger() { }

	public void OnExplode()
	{
		if ( !IsDud )
		{
			if ( IsProxy )
				return;

			ExplodeEffects();
			GameObject.Destroy();
		}
		else
		{
			IsArmed = false;
		}
	}

	public void StartDetonating()
	{
		_detonatedAt = 0;
		IsDetonating = true;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !IsArmed || IsDetonating )
			return;

		if ( !_grubs.Any( g => !g.CharacterController.Velocity.IsNearlyZero( .1f ) ) )
			return;

		StartDetonating();
		OnTrigger();
	}

	protected override void OnStart()
	{
		base.OnStart();

		_createdAt = 0f;
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !other.GameObject.Tags.Has( "player" ) )
			return;

		if ( IsDetonating )
			return;

		if ( !other.GameObject.Components.TryGet<Grub>( out var grub, FindMode.EverythingInSelfAndParent ) )
			return;

		_grubs.Add( grub );

		if ( IsArmed )
		{
			_detonatedAt = 0;
			IsDetonating = true;
			OnTrigger();
		}
	}

	public void OnTriggerExit( Collider other )
	{
		if ( !other.GameObject.Tags.Has( "player" ) )
			return;

		if ( !other.GameObject.Components.TryGet<Grub>( out var grub, FindMode.EverythingInSelfAndParent ) )
			return;

		_grubs.Remove( grub );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsArmed )
		{
			if ( _createdAt > ArmTime && !IsDetonating )
			{
				IsArmed = true;
				OnArm();
			}

			return;
		}

		if ( IsDetonating )
		{
			if ( _detonatedAt > DetonateTime )
			{
				OnExplode();
			}
		}
	}

	[Broadcast]
	public void ExplodeEffects()
	{
		var projectile = Components.Get<Projectile>();
		ExplosionHelper.Instance.Explode( this, Transform.Position, Radius, Damage, projectile?.GrubGuid ?? Guid.Empty, projectile?.GrubName ?? string.Empty );
		Sound.Play( ExplosionSound );

		if ( Particles is null )
			return;

		var sceneParticles = ParticleHelper.Instance.PlayInstantaneous( Particles, Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( Radius / 2f, 0, 0 ) );
	}
}
