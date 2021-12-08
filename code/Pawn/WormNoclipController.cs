using Sandbox;

namespace TerryForm.Pawn
{
	[Library]
	public class WormNoclipController : BasePlayerController
	{
		public override void Simulate()
		{
			var vel = (Vector3.Forward * -Input.Left);

			if ( Input.Down( InputButton.Jump ) )
			{
				vel += Vector3.Up * 1;
			}

			vel = vel.Normal * 2000;

			if ( Input.Down( InputButton.Run ) )
				vel *= 5.0f;

			if ( Input.Down( InputButton.Duck ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Position += Velocity * Time.Delta;
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			WishVelocity = Velocity;
			GroundEntity = null;
			BaseVelocity = Vector3.Zero;

			SetTag( "noclip" );
		}

	}
}
