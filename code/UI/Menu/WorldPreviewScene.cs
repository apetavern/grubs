using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Grubs.UI.Menu
{
	public class WorldPreviewScene : Panel
	{
		private ScenePanel renderScene;
		private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
		private float renderSceneDistance = 50;
		private Vector3 renderScenePos => Vector3.Up * 18 + renderSceneAngles.Direction * -renderSceneDistance;

		AnimSceneObject worm;

		public WorldPreviewScene()
		{
			Build();
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			Build();
		}

		public override void Tick()
		{
			base.Tick();
			worm.Update( Time.Delta );
			worm.SetAnimBool( "grounded", true );
			worm.SetAnimFloat( "incline", 0f );
		}

		public void Build()
		{
			renderScene?.Delete( true );

			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				SceneObject.CreateModel( Model.Load( "models/menu/menubg.vmdl" ), Transform.Zero.WithScale( 1 ).WithPosition( Vector3.Down * 4 ) );
				worm = new AnimSceneObject( Model.Load( "models/citizenworm.vmdl" ),
					Transform.Zero.WithScale( 1f ).WithPosition( new Vector3( -64, 32, 4 ) ).WithRotation( Rotation.From( 0, -135, 0 ) )
					);

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
				renderScene.CameraPosition = new Vector3( -100, -50, 25 );
				renderScene.CameraRotation = Rotation.From( 0, 75, 0 );
				renderSceneAngles = renderScene.CameraRotation.Angles();
				renderScene.AmbientColor = new Color( .25f, .15f, .15f ) * 0.25f;
			}
		}
	}
}
