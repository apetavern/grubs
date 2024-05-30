using static Sandbox.Component;

namespace Grubs;

[Title( "Grubs - Bouncy" ), Category( "Equipment" )]
public sealed class Bouncy : Component, ICollisionListener
{
	[Property] public Rigidbody Body { get; set; }
	[Property] public float DampingFactor { get; set; } = 0.8f;
	[Property] private bool Reflect { get; set; } = true;

	public void OnCollisionStart( Collision other )
	{
		var speed = other.Contact.Speed.Length;
		var direction = Reflect ? Vector3.Reflect( other.Contact.Speed.Normal, other.Contact.Normal ) : -Body.Velocity.Normal;
		Body.Velocity = direction * speed * DampingFactor;
	}
}
