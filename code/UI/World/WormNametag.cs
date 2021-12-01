using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using TerryForm.Pawn;

namespace TerryForm.UI.World
{
	public class WormNametag : WorldPanel
	{
		public Worm Worm { get; set; }

		private Vector3 Offset => Vector3.Up * 48;

		private Label name;
		private Label health;

		public WormNametag()
		{
			StyleSheet.Load( "/UI/World/WormNametag.scss" );

			name = Add.Label( "Froggy", "worm-name" );
			health = Add.Label( "100", "worm-health" );

			float width = 500;
			float height = 250;

			float halfWidth = width / 2;
			float halfHeight = height / 2;

			PanelBounds = new Rect( -halfWidth, -halfHeight, width, height );

			SceneObject.ZBufferMode = ZBufferMode.None;
			SceneObject.Flags.BloomLayer = false;
		}

		private void Update()
		{
			//
			// Update data...
			//

			name.Text = Worm.Name.ToString();
			health.Text = Worm.Health.ToString();
		}

		public override void Tick()
		{
			base.Tick();

			if ( !Worm.IsValid || Worm is null )
				Delete( true );

			Position = Worm.EyePos + Offset;
			Rotation = Rotation.LookAt( Vector3.Right );

			if ( Local.Pawn.Camera is TerryForm.Pawn.Camera camera )
			{
				WorldScale = (1.0f + camera.DistanceRange.LerpInverse( -camera.Position.y )) * 5f;
			}

			Update();
		}
	}
}
