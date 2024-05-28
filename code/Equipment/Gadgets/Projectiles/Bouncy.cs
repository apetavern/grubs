using static Sandbox.Component;

namespace Grubs;

[Title( "Grubs - Bouncy" ), Category( "Equipment" )]
public sealed class Bouncy : Component, ICollisionListener
{
	[Property] public Rigidbody Body { get; set; }
	[Property] public float DampingFactor { get; set; } = 0.8f;

	public void OnCollisionStart( Collision other )
	{
		var speed = other.Contact.Speed.Length;
		var direction = Vector3.Reflect( other.Contact.Speed.Normal, other.Contact.Normal );
		Body.Velocity = direction * speed * DampingFactor;
	}
}
