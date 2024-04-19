using Grubs.Pawn;
using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Terrain" )]
public partial class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }

	[Property] public required Sdf2DWorld SdfWorld { get; set; }
	[Property] public Water Water { get; set; }

	public GrubsTerrain()
	{
		Instance = this;
	}

	public async void Init()
	{
		Game.SetRandomSeed( (int)(DateTime.Now - DateTime.UnixEpoch).TotalSeconds );

		await SdfWorld.ClearAsync();
		SdfWorld.Transform.Rotation = Rotation.FromRoll( 90f );

		SetupGeneratedWorld();
	}

	public Vector3 FindSpawnLocation( float size = 16f )
	{
		var retries = 0;
		var fallbackPosition = new Vector3();

		var maxWidth = GrubsConfig.TerrainLength;
		var maxHeight = GrubsConfig.TerrainHeight;

		while ( retries < 1000 )
		{
			retries++;

			var randX = Game.Random.Int( maxWidth ) - maxWidth / 2;
			var randZ = Game.Random.Int( maxHeight );
			var startPos = new Vector3( randX, 512, randZ );

			var tr = Scene.Trace.Ray( startPos, startPos + Vector3.Down * maxHeight )
				.WithAnyTags( "solid", "player" )
				.Size( size )
				.Run();

			if ( tr.Hit && !tr.StartedSolid )
			{
				if ( tr.GameObject.Components.TryGet( out Grub _, FindMode.EverythingInSelfAndAncestors ) )
					continue;

				if ( PointInside( tr.EndPosition ) )
					continue;

				if ( Vector3.GetAngle( Vector3.Up, tr.Normal ) > 80f )
					continue;

				return tr.EndPosition;
			}
		}

		return fallbackPosition;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = global::Transform.Zero;

		float l = GrubsConfig.TerrainLength;
		float h = GrubsConfig.TerrainHeight;
		Gizmo.Draw.LineBBox( new BBox( new Vector3( -l / 2f, 512f - 32f, 0f ), new Vector3( l / 2f, 512f + 96f, h ) ) );
	}

	public bool PointInside( Vector3 point )
	{
		var tr = Scene.Trace.Ray( point, point + Vector3.Right * 64f )
			.WithAnyTags( "solid" )
			.Size( 2f )
			.Run();

		return tr.Hit;
	}

	[Broadcast]
	public void SendMeMissing( Guid to, int clearCount, int modificationCount )
	{
		if ( Connection.Local != Connection.Host )
			return;

		var conn = Connection.Find( to );
		Log.Info( $"Want to send missing to {conn.DisplayName} : {conn.Name} with {clearCount}-{modificationCount}" );
		SdfWorld.RequestMissing( conn, clearCount, modificationCount );
	}

	private float _notifiedMissingModifications = float.PositiveInfinity;

	[Broadcast]
	public void WriteRpc( Guid guid, byte[] bytes )
	{
		if ( Connection.Local.Id != guid )
			return;

		var byteStream = ByteStream.CreateReader( bytes );
		if ( SdfWorld.Read( ref byteStream ) )
		{
			_notifiedMissingModifications = float.PositiveInfinity;
			return;
		}

		if ( _notifiedMissingModifications >= 0.5f )
		{
			_notifiedMissingModifications = 0f;

			SendMeMissing( Connection.Local.Id, SdfWorld.ClearCount, SdfWorld.ModificationCount );
		}
	}
}
