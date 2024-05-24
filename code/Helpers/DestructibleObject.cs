using Grubs.Common;
using Sandbox;

namespace Grubs;

public sealed class DestructibleObject : Component
{
	[Property, ToggleGroup( "HasDamageMeshes" )]
	private bool HasDamageMeshes {  get; set; }

	[Property, ToggleGroup( "HasDamageMeshes" )]
	private GameObject LeftSide { get; set; }

	[Property, ToggleGroup( "HasDamageMeshes" )]
	private GameObject RightSide { get; set; }

	[Property, RequireComponent] 
	private Health Health { get; set; }

	protected override void OnStart()
	{
		Health.ObjectDied += GameObject.Destroy;
		Health.ObjectDamaged += OnDamaged;
	}

	protected override void OnDestroy()
	{
		Health.ObjectDied -= GameObject.Destroy;
		Health.ObjectDamaged -= OnDamaged;
	}

	public void OnDamaged( GrubsDamageInfo damageInfo )
	{
		if ( !HasDamageMeshes )
			return;

		var damageDirection = (Transform.Position - damageInfo.WorldPosition).Normal;
		var dotForward = Vector3.Dot( Transform.Rotation.Forward, damageDirection );
		var dotUp = Vector3.Dot( Transform.Rotation.Up, damageDirection );
		var isLeftSide = dotForward < 0;

		var side = isLeftSide ? LeftSide : RightSide;
		var renderer = side.Components.Get<SkinnedModelRenderer>();

		if ( damageInfo.Damage < 20f && Health.CurrentHealth > Health.MaxHealth / 3f )
		{
			renderer.SetBodyGroup( isLeftSide ? "Left" : "Right", dotUp > 0 ? 1 : 2 );
		}
		else
		{
			renderer.SetBodyGroup( isLeftSide ? "Left" : "Right", 3 );
			side.Components.Get<ModelCollider>( true ).Enabled = false;
		}
	}
}
