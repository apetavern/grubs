using Grubs.Common;
using Sandbox;

namespace Grubs;

public sealed class DestructibleObject : Component
{
	[Property, ToggleGroup( "HasDamageMeshes" )]
	bool HasDamageMeshes {  get; set; }

	[Property, ToggleGroup( "HasDamageMeshes" )] 
	SkinnedModelRenderer SkinnedModelRenderer { get; set; }

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
				if ( dotUp > 0 )
				{
					SkinnedModelRenderer.SetBodyGroup( "Left", 1 );
				}
				else
				{
					SkinnedModelRenderer.SetBodyGroup( "Left", 2 );
				}
			}
			else
			{
				if ( dotUp > 0 )
				{
					SkinnedModelRenderer.SetBodyGroup( "Right", 1 );
				}
				else
				{
					SkinnedModelRenderer.SetBodyGroup( "Right", 2 );
				}
			}
		}
	}
}
