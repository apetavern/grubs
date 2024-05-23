using Grubs.Common;
using Grubs.Helpers;
using Grubs.Pawn;
using Sandbox;
using System.Text;

namespace Grubs.Equipment.Ground;

[Title( "Grubs - Proximity Explosive" ), Category( "Equipment" )]

public partial class ProximityExplosiveComponent : Component, Component.ICollisionListener, IResolvable
{
	[Property] public bool IsDud { get; set; }
	[Property] public bool IsArmed { get; set; }
	[Property] public bool IsDetonating { get; set; }
	[Property] public float Radius { get; set; } = 64.0f;
	[Property] public float Damage { get; set; } = 100.0f;
	[Property] public float ArmTime { get; set; } = 5.0f;
	[Property] public float DetonateTime { get; set; } = 5.0f;
	[Property, ResourceType( "sound" )] public string ExplosionSound { get; set; } = "";
	[Property, ResourceType( "vpcf" )] public ParticleSystem Particles { get; set; }

	[Property] public ModelCollider Collider { get; set; }

	private TimeSince _createdAt { get; set; }
	private TimeSince _detonatedAt { get; set; }

	public bool Resolved => !IsDetonating;

	public virtual void OnArm() { }
	public virtual void OnTrigger() { }

	public virtual void OnExplode() 
	{
		if (!IsDud)
			ExplodeEffects();

		GameObject.Destroy();
	}

	protected override void OnStart()
	{
		base.OnStart();

		_createdAt = 0f;
	}

	public void OnCollisionStart( Collision other )
	{
		// Check if the Collision other is a Grub
		if ( !other.Other.GameObject.Tags.Has( "player" ) )
			return;

		if ( IsDetonating || !IsArmed )
			return;

		IsDetonating = true;
		_detonatedAt = 0;
		OnTrigger();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!IsArmed)
		{
			if (_createdAt > ArmTime)
			{
				IsArmed = true;
				OnArm();
			}

			return;
		}

		if (IsDetonating)
		{
			if (_detonatedAt > DetonateTime)
			{
				OnExplode();
			}
		}
	}

	[Broadcast]
	public void ExplodeEffects()
	{
		ExplosionHelperComponent.Instance.Explode( this, Transform.Position, Radius, Damage );
		Sound.Play( ExplosionSound );

		if ( Particles is null )
			return;

		var sceneParticles = ParticleHelperComponent.Instance.PlayInstantaneous( Particles, Transform.World );
		sceneParticles.SetControlPoint( 1, new Vector3( Radius / 2f, 0, 0 ) );
	}
}
