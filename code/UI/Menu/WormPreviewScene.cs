using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Grubs.UI.Menu
{
	public partial class WormPreviewScene : Panel
	{
		private ScenePanel renderScene;
		private AnimSceneObject wormModel;
		private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
		private float renderSceneDistance = 50;
		private Vector3 renderScenePos => Vector3.Up * 18 + renderSceneAngles.Direction * -renderSceneDistance;

		public override void OnButtonEvent( ButtonEvent e )
		{
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
			base.Tick();

			if ( renderScene == null ) return;

			if ( HasMouseCapture )
			{
				/*renderSceneAngles.pitch += Mouse.Delta.y;*/
				renderSceneAngles.yaw -= Mouse.Delta.x;
				renderSceneAngles.pitch = 0;
			}

			renderScene.CameraPosition = renderScene.CameraPosition.LerpTo( renderScenePos, 10f * Time.Delta );
			renderScene.CameraRotation = Rotation.Lerp( renderScene.CameraRotation, Rotation.From( renderSceneAngles ), 15f * Time.Delta );
		}

		public void Build()
		{
			renderScene?.Delete( true );

			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				// Dress Worm Here

				wormModel = new AnimSceneObject( Model.Load( "models/citizenworm.vmdl" ), Transform.Zero.WithScale( 1 ).WithPosition( Vector3.Down * 4 ) );

				var skycolor = Color.Orange;

				var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
				if ( sceneLight.IsValid() )
				{
					skycolor = sceneLight.SkyColor;
				}

				Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
				Light.Point( Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
				Light.Point( Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, skycolor * 20.0f );
				Light.Point( Vector3.Up * 75.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 100.0f + Vector3.Up, 200, Color.Yellow * 15.0f );

				renderScene = Add.ScenePanel( SceneWorld.Current, renderScenePos, Rotation.From( renderSceneAngles ), 75 );
				renderScene.Style.Width = Length.Percent( 100 );
				renderScene.Style.Height = Length.Percent( 100 );
				renderScene.CameraPosition = new Vector3( 0, 0, 0 );
				renderScene.CameraRotation = Rotation.From( 0, 210, 0 );
				renderSceneAngles = renderScene.CameraRotation.Angles();
			}
		}

		[Event.Tick.Client]
		public void TickWorm()
		{
			if ( wormModel == null ) return;

			wormModel.SetAnimBool( "grounded", true );
			wormModel.SetAnimFloat( "incline", 0f );

			wormModel.Update( RealTime.Delta );
		}

	}
}
