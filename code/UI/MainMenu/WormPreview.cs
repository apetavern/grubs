namespace Grubs.UI;

public class WormPreview : Panel
{
	private readonly ScenePanel _renderScene;
	private readonly SceneModel _worm;
	private Angles _renderSceneAngles = new( 25f, 0f, 0f );
	private float _renderSceneDistance = 100f;
	private Vector3 _renderScenePosition = new( -100f, -50f, 25f );
	private float _yaw;

	public WormPreview()
	{
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );
		Style.Position = PositionMode.Absolute;
		Style.ZIndex = -10;

		_renderScene?.Delete( true );

		var sceneWorld = new SceneWorld();
		var map = new SceneMap( sceneWorld, "maps/gr_menu" );

		_worm = new SceneModel( sceneWorld, Model.Load( "models/citizenworm.vmdl" ),
			Transform.Zero.WithScale( 1f ).WithPosition( new Vector3( -64, 32, 4 ) ).WithRotation( Rotation.From( 0, -135, 0 ) ) );

		var skycolor = Color.Orange;
		var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
		if ( sceneLight.IsValid() )
			skycolor = sceneLight.SkyColor;

		// Right side light
		// new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
		// Background light
		_ = new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 150.0f, 200, Color.White * 2.0f );
		// Foreground light
		_ = new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 80.0f, 200, Color.White * 5.0f );
		// Back/rim light
		_ = new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, skycolor * 3.0f );
		// Soft right side fill light
		_ = new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Right * 100.0f, 200, skycolor * 3.0f );
		// Right side yellow light
		_ = new SceneLight( sceneWorld, Vector3.Up * 100.0f + Vector3.Up, 200, Color.Yellow * 2.0f );

		_renderScene = Add.ScenePanel( sceneWorld, _renderScenePosition, Rotation.From( _renderSceneAngles ), 75 );
		_renderScene.Style.Width = Length.Percent( 100 );
		_renderScene.Style.Height = Length.Percent( 100 );
		_renderScene.Camera.Rotation = Rotation.From( 0, 75, 0 );
		_renderSceneAngles = _renderScene.Camera.Rotation.Angles();
		_renderScene.Camera.AmbientLightColor = new Color( .25f, .15f, .15f ) * 0.5f;
	}

	public override void OnButtonEvent( ButtonEvent e )
	{
		// CaptureMouseInput doesn't work wtf scam?
		if ( e.Button == "mouseleft" )
			SetMouseCapture( e.Pressed );

		base.OnButtonEvent( e );
	}

	public override void OnMouseWheel( float value )
	{
		_renderSceneDistance += value * 3;
		_renderSceneDistance = _renderSceneDistance.Clamp( 10, 200 );
		base.OnMouseWheel( value );
	}

	public override void Tick()
	{
		if ( _renderScene == null )
			return;

		if ( HasMouseCapture )
		{
			_yaw -= Mouse.Delta.x;
			_renderSceneAngles.pitch = 0;
		}

		_yaw = _yaw.Clamp( -200, -130 );

		float yawRad = MathX.DegreeToRadian( _yaw );
		float height = 16;

		_renderScene.Camera.Position = _worm.Position + new Vector3(
			MathF.Sin( yawRad ) * _renderSceneDistance,
			MathF.Cos( yawRad ) * _renderSceneDistance,
			height
		);

		var wormEyePos = _worm.Position + _worm.Rotation.Up * 24;
		wormEyePos += _worm.Rotation.Right * 4;
		_renderScene.Camera.Rotation = Rotation.LookAt( (wormEyePos - _renderScene.Camera.Position).Normal );

		Animate();
	}

	private void Animate()
	{
		_worm.Update( Time.Delta );
		_worm.SetAnimParameter( "grounded", true );
		_worm.SetAnimParameter( "incline", 0f );
	}
}
