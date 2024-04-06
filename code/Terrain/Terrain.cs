using Sandbox.Sdf;

namespace Grubs.Terrain;

[Title( "Grubs - Terrain" )]
public class GrubsTerrain : Component
{
	public static GrubsTerrain Instance { get; set; }
	[Property] public required Sdf2DWorld SdfWorld { get; set; }
	public Sdf2DLayer Sand { get; set; } = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );

	public GrubsTerrain()
	{
		Instance = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
		{
			Log.Info( SdfWorld );
		}
	}

	public async void Init()
	{
		await SdfWorld.ClearAsync();
		SdfWorld.Transform.Rotation = Rotation.FromRoll( 90f );

		var circleSdf = new CircleSdf( new Vector2( 0f, 0f ), 128f );
		await SdfWorld.AddAsync( circleSdf, Sand );
	}

	[Broadcast]
	public void SubtractCircle( Vector2 center, float radius )
	{
		if ( Connection.Local != Connection.Host )
			return;

		var circleSdf = new CircleSdf( center, radius );
		var translatedCircleSdf = circleSdf.Translate( new Vector2( 0f, -Transform.Position.z ) );
		SdfWorld?.SubtractAsync( translatedCircleSdf, Sand );
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
