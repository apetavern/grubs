using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
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

			name = Add.Label( "Worm Name", "worm-name" );
			health = Add.Label( "0", "worm-health" );

			float width = 500;
			float height = 250;

			float halfWidth = width / 2;
			float halfHeight = height / 2;

			PanelBounds = new Rect( -halfWidth, -halfHeight, width, height );

			SceneObject.ZBufferMode = ZBufferMode.None;
			SceneObject.Flags.BloomLayer = false;
		}

		private void UpdateLabels()
		{
			name.Text = Worm.Name.ToString();
			health.Text = Worm.Health.ToString();
		}

		private void Move()
		{
			Position = Worm.EyePos + Offset;
			Rotation = Rotation.LookAt( Vector3.Right );

			if ( Local.Pawn.Camera is TerryForm.Pawn.Camera camera )
			{
				WorldScale = (1.0f + camera.DistanceRange.LerpInverse( -camera.Position.y )) * 5f;
			}
		}

		public override void Tick()
		{
			base.Tick();

			Move();
			UpdateLabels();
		}
	}
}
