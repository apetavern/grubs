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

		public WormNametag()
		{
			StyleSheet.Load( "/UI/World/WormNametag.scss" );
			Add.Label( "Froggy", "worm-name" );

			float width = 350;
			float height = 125;

			float halfWidth = width / 2;
			float halfHeight = height / 2;

			PanelBounds = new Rect( -halfWidth, -halfHeight, width, height );

			SceneObject.ZBufferMode = ZBufferMode.None;
			WorldScale = 5f;
		}

		private void Update()
		{
			//
			// Update data...
			//
		}

		public override void Tick()
		{
			base.Tick();

			Position = Worm.EyePos + Offset;
			Rotation = Rotation.LookAt( Vector3.Right );
		}
	}
}
