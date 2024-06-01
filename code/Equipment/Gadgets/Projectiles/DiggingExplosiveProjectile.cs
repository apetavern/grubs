using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Terrain;

namespace Grubs;

[Title( "Grubs - Digging Explosive Projectile" ), Category( "Equipment" )]
public sealed class DiggingExplosiveProjectile : TargetedProjectile
{
	[Property] public float DigWidth { get; set; } = 10f;
	[Property] public float DigLength { get; set; } = 50f;
	[Property] public ExplosiveProjectile Explosive { get; set; }
	[Property] public PhysicsProjectile Physics { get; set; }
	[Property, Description( "The seconds after making contact with terrain before exploding" )] public float TimeBeforeDetonation { get; set; } = 4f;

	private bool _isArmed = false; // Whether the projectile has made contact with terrain
	private TimeSince _timeSinceArmed = 0f;

	public override void ShareData()
	{
		base.ShareData();
		Transform.Position = ProjectileTarget.WithZ( GrubsConfig.TerrainHeight * 2f );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var startPos = Transform.Position;
		var endPos = startPos + Vector3.Down * DigLength;

		var tr = Scene.Trace.Body( Physics.PhysicsBody.PhysicsBody, endPos )
			.WithAnyTags( "player", "solid" )
			.WithoutTags( "dead" )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.Run();

		if ( tr.Hit )
		{
			var hit = tr.GameObject;
			if ( hit.Tags.Has( "terrain" ) )
			{
				if ( !_isArmed )
				{
					_timeSinceArmed = 0f;
					_isArmed = true;
				}
			}
			else
			{
				Explosive.Explode();
				return;
			}
		}

		if ( (_isArmed && _timeSinceArmed > TimeBeforeDetonation) || Vector3.DistanceBetween( Transform.Position, ProjectileTarget ) < 10f )
		{
			Explosive.Explode();
			return;
		}

		using ( Rpc.FilterInclude( c => c.IsHost ) )
		{
			GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ), DigWidth, 1 );
			GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
				new Vector2( endPos.x, endPos.z ),
				DigWidth + 8f );
		}
	}
}
