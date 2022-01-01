﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Grubs.Pawn;

namespace Grubs.UI.World
{
	public class WormNametag : WorldPanel
	{
		public Worm Worm { get; set; }

		private Vector3 Offset => Vector3.Up * 72;

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
			if ( !Worm.IsValid || Worm is null )
			{
				Delete( true );
				return;
			}

			// We can't use eye pos here, because it doesn't get set until
			// the worm has been simulated - which means they'll be inconsistent
			// until everyone's had at least 1 turn
			Position = Worm.Position + Offset;
			Rotation = Rotation.LookAt( Vector3.Right );

			if ( Local.Pawn.Camera is Grubs.Pawn.Camera camera )
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
