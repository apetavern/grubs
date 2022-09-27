namespace Grubs.UI.Menu;

public class WormPreviewScene : Panel
{
	private ScenePanel renderScene = null!;
	private Angles renderSceneAngles = new( 25f, 0f, 0f );
	private float renderSceneDistance = 50f;
	private Vector3 renderScenePosition => new Vector3( -100f, -50f, 25f );

	float yaw;

	SceneModel stage = null!;
	SceneModel worm = null!;

	public WormPreviewScene()
	{
		Build();
	}

	private void Build()
	{
		renderScene?.Delete( true );

		var sceneWorld = new SceneWorld();
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );

		stage = new SceneModel( sceneWorld, Model.Load( "models/menu/menubg.vmdl" ), 
			Transform.Zero.WithScale( 1f ).WithPosition( Vector3.Down * 4 ) );
		worm = new SceneModel( sceneWorld, Model.Load( "models/citizenworm.vmdl" ),
			Transform.Zero.WithScale( 1f ).WithPosition( new Vector3( -64, 32, 4 ) ).WithRotation( Rotation.From( 0, -135, 0 ) ) );

		var skycolor = Color.Orange;
		var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
		if ( sceneLight.IsValid() )
		{
			skycolor = sceneLight.SkyColor;
		}

		new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, skycolor * 20.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 100.0f + Vector3.Up, 200, Color.Yellow * 15.0f );

		renderScene = Add.ScenePanel( sceneWorld, renderScenePosition, Rotation.From( renderSceneAngles ), 75 );
		renderScene.Style.Width = Length.Percent( 100 );
		renderScene.Style.Height = Length.Percent( 100 );
		renderScene.Camera.Rotation = Rotation.From( 0, 75, 0 );
		renderSceneAngles = renderScene.Camera.Rotation.Angles();
		renderScene.Camera.AmbientLightColor = new Color( .25f, .15f, .15f ) * 0.5f;
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
		renderSceneDistance += value * 3;
		renderSceneDistance = renderSceneDistance.Clamp( 10, 200 );
		base.OnMouseWheel( value );
	}

	public override void Tick()
	{
		if ( renderScene == null ) 
			return;

		if ( HasMouseCapture )
		{
			yaw -= Mouse.Delta.x;
			renderSceneAngles.pitch = 0;
		}

		yaw = yaw.Clamp( -155, -115 );

		float yawRad = MathX.DegreeToRadian( yaw );
		float height = 16;

		renderScene.Camera.Position = worm.Position + new Vector3(
			MathF.Sin( yawRad ) * renderSceneDistance,
			MathF.Cos( yawRad ) * renderSceneDistance,
			height
		);

		var wormEyePos = worm.Position + worm.Rotation.Up * 24;
		wormEyePos += worm.Rotation.Right * 4;
		renderScene.Camera.Rotation = Rotation.LookAt( (wormEyePos - renderScene.Camera.Position).Normal );

		Animate();
	}

	private void Animate()
	{
		worm.Update( Time.Delta );
		worm.SetAnimParameter( "grounded", true );
		worm.SetAnimParameter( "incline", 0f );
	}
}
