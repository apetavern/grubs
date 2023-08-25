using Sandbox;
using Sandbox.Sdf;

namespace Grubs.UI;

public class WorldScene : Panel
{
	public bool ShowLogo { get; set; }

	private readonly GrubPreview _grubPreview;
	private readonly ScenePanel _renderScene;
	private Sdf2DWorld _sdfWorld;
	private float _renderSceneDistance = 100f;
	private float _yaw = -175;

	public WorldScene()
	{
		StyleSheet.Load( "UI/MainMenu/WorldScene/WorldScene.cs.scss" );

		_renderScene?.Delete( true );

		var sceneWorld = new SceneWorld();
		var map = new SceneMap( sceneWorld, "maps/gr_menu" );

		_grubPreview = new GrubPreview( sceneWorld );
		_grubPreview.Grub.Position = new Vector3( -64, 32, 6 );
		_grubPreview.Grub.Rotation = Rotation.From( 0, -135, 0 );
		AddChild( _grubPreview );

		_renderScene = Add.ScenePanel( sceneWorld, Vector3.One, Rotation.Identity, 75, "renderScene" );
		_renderScene.Camera.AmbientLightColor = new Color( .25f, .15f, .15f ) * 0.5f;
		_renderScene.Camera.Position = _grubPreview.Grub.Position + new Vector3(
			MathF.Sin( _yaw ) * _renderSceneDistance,
			MathF.Cos( _yaw ) * _renderSceneDistance
		);

		map.World.GradientFog.Enabled = true;
		map.World.GradientFog.Color = new Color32( 57, 48, 69 );
		map.World.GradientFog.MaximumOpacity = 0.6f;
		map.World.GradientFog.StartHeight = 10;
		map.World.GradientFog.EndHeight = 9000;
		map.World.GradientFog.DistanceFalloffExponent = 4;
		map.World.GradientFog.VerticalFalloffExponent = 4;
		map.World.GradientFog.StartDistance = 250;
		map.World.GradientFog.EndDistance = 600;
	}

	private async Task SetupWorld()
	{
		try
		{
			var sand = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );
			var rock = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/rock.sdflayer" );

			var mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + ".png" );
			var mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
			var transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, 0 ) );

			await _sdfWorld.AddAsync( transformedSdf, sand );

			mapSdfTexture = await Texture.LoadAsync( FileSystem.Mounted, "textures/texturelevels/" + GrubsConfig.WorldTerrainTexture.ToString() + "_back.png" );
			mapSdf = new TextureSdf( mapSdfTexture, 10, mapSdfTexture.Width * 2f, pivot: 0f );
			transformedSdf = mapSdf.Transform( new Vector2( -GrubsConfig.TerrainLength / 2f, 0 ) );

			await _sdfWorld.AddAsync( transformedSdf, rock );
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}

	public void DrawOnTerrain( Vector2 prevPos, Vector2 nextPos )
	{
		if ( _sdfWorld == null )
		{
			return;
		}

		var plane = new Plane( _sdfWorld.Position, _sdfWorld.Rotation.Up );
		var prevRay = _renderScene.Camera.GetRay( prevPos );
		var nextRay = _renderScene.Camera.GetRay( nextPos );

		if ( !plane.TryTrace( prevRay, out var prevWorldPos, true )
			|| !plane.TryTrace( nextRay, out var nextWorldPos, true ) )
		{
			return;
		}

		var prevLocalPos = _sdfWorld.Transform.PointToLocal( prevWorldPos );
		var nextLocalPos = _sdfWorld.Transform.PointToLocal( nextWorldPos );

		var sand = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/sand.sdflayer" );
		var scorch = ResourceLibrary.Get<Sdf2DLayer>( "materials/sdf/scorch.sdflayer" );

		var sdf = new LineSdf( prevLocalPos, nextLocalPos, 32f );

		_ = _sdfWorld.SubtractAsync( sdf, sand );
		_ = _sdfWorld.AddAsync( sdf.Expand( 8f ), scorch );
	}

	public override void OnButtonEvent( ButtonEvent e )
	{
		// CaptureMouseInput doesn't work wtf scam?
		if ( e.Button == "mouseleft" )
		{
			SetMouseCapture( e.Pressed );
		}

		base.OnButtonEvent( e );
	}

	public override void OnMouseWheel( float value )
	{
		_renderSceneDistance += value * 3;
		_renderSceneDistance = _renderSceneDistance.Clamp( 50, 150 );
		base.OnMouseWheel( value );
	}

	public override void Tick()
	{
		if ( _renderScene == null )
			return;

		if ( ShowLogo && _sdfWorld == null )
		{
			_sdfWorld = new Sdf2DWorld( _renderScene.World )
			{
				Scale = 1f / 24f,
				Rotation = Rotation.FromRoll( 90f ),
				Position = _grubPreview.Grub.Position + Vector3.Up * 32f
			};

			_ = SetupWorld();
		}

		_sdfWorld?.Update();

		if ( HasMouseCapture )
			_yaw -= Mouse.Delta.x * 0.05f;

		_yaw = _yaw.Clamp( -200, -130 );

		float yawRad = MathX.DegreeToRadian( _yaw );
		float height = 16;

		var currentPosition = _renderScene.Camera.Position;
		_renderScene.Camera.Position = currentPosition.LerpTo( _grubPreview.Grub.Position + new Vector3(
			MathF.Sin( yawRad ) * _renderSceneDistance,
			MathF.Cos( yawRad ) * _renderSceneDistance,
			height
		), Time.Delta * 4.0f );

		var wormEyePos = _grubPreview.Grub.Position + _grubPreview.Grub.Rotation.Up * 24;
		wormEyePos += _grubPreview.Grub.Rotation.Right * 4;
		_renderScene.Camera.Rotation = Rotation.LookAt( (wormEyePos - _renderScene.Camera.Position).Normal );
	}
}
