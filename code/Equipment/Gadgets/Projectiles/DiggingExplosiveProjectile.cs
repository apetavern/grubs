using Grubs.Equipment.Gadgets.Projectiles;
using Grubs.Terrain;
using static Sandbox.Component;

namespace Grubs;

[Title( "Grubs - Digging Explosive Projectile" ), Category( "Equipment" )]
public sealed class DiggingExplosiveProjectile : TargetedProjectile
{
	[Property] public float DigWidth { get; set; } = 10f;
	[Property] public float DigLength { get; set; } = 50f;
	[Property] public ExplosiveProjectile Projectile { get; set; }
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

		var tr = Scene.Trace.Ray( Transform.Position, endPos + Vector3.Down * 10f )
				.WithAnyTags( "player", "solid" )
				.WithoutTags( "dead" )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.Run();

		LastPosition = tr.EndPosition;

		if ( tr.Hit )
		{
			Projectile.Explode();
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

	private Vector3 LastPosition { get; set; }
	protected override void DrawGizmos()
	{
		Gizmo.Transform = global::Transform.Zero;

		Gizmo.Draw.LineSphere( LastPosition, 8f );
	}
}
