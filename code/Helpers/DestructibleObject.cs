using Grubs.Common;
using Sandbox;

namespace Grubs;

public sealed class DestructibleObject : Component
{
	[Property, ToggleGroup( "HasDamageMeshes" )]
	bool HasDamageMeshes {  get; set; }

	[Property, ToggleGroup( "HasDamageMeshes" )] 
	GameObject LeftSide { get; set; }

	[Property, ToggleGroup( "HasDamageMeshes" )]
	GameObject RightSide { get; set; }

	[RequireComponent, Property] Health health { get; set; }

	protected override void OnStart()
	{
		health.ObjectDied += GameObject.Destroy;
		health.ObjectDamaged += OnDamaged;
	}

	protected override void OnDestroy()
	{
		health.ObjectDied -= GameObject.Destroy;
		health.ObjectDamaged -= OnDamaged;
	}

	public void OnDamaged( GrubsDamageInfo damageInfo )
	{
		if ( HasDamageMeshes )
		{
			var damageDirection = (Transform.Position - damageInfo.WorldPosition).Normal;

			float dotForward = Vector3.Dot( Transform.Rotation.Forward, damageDirection );

			float dotUp = Vector3.Dot( Transform.Rotation.Up, damageDirection );

			if ( dotForward < 0 )
			{
				if ( damageInfo.Damage < 20f && health.CurrentHealth > health.MaxHealth/3f )
				{
					if ( dotUp > 0 )
					{
						LeftSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Left", 1 );
					}
					else
					{
						LeftSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Left", 2 );
					}
				}
				else
				{
					LeftSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Left", 3 );
					LeftSide.Components.Get<ModelCollider>(true).Enabled = false;
				}
			}
			else
			{
				if ( damageInfo.Damage < 20f && health.CurrentHealth > health.MaxHealth / 3f )
				{
					if ( dotUp > 0 )
					{
						RightSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Right", 1 );
					}
					else
					{
						RightSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Right", 2 );
					}
				}
				else
				{
					RightSide.Components.Get<SkinnedModelRenderer>().SetBodyGroup( "Right", 3 );
					RightSide.Components.Get<ModelCollider>( true ).Enabled = false;
				}
			}
		}
	}
}
