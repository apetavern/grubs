using Grubs.Common;
using Sandbox;
using System.IO.Pipes;

namespace Grubs;

public sealed class RopeBehaviorComponent : Component
{
	[Property] public GameObject HookObject { get; set; }

	[Property] public List<GameObject> CornerObjects { get; set; } = new List<GameObject>();

	[Property] LineRenderer RopeRenderer { get; set; }

	SpringJoint jointComponent { get; set; }

	public Vector3 HookDirection;

	GameObject MuzzlePoint { get; set; }

	Mountable mountComponent { get; set; }

	protected override void OnAwake()
	{
		jointComponent = Components.Get<SpringJoint>();
		jointComponent.MaxLength = Vector3.DistanceBetween( Transform.Position, HookObject.Transform.Position );
		jointComponent.Body = HookObject;
		ropeLength = jointComponent.MaxLength;

		mountComponent = Components.Get<Mountable>();

		MuzzlePoint = new GameObject( true, "MuzzlePoint" );
	}

	[Property] float ropeLength {  get; set; }

	protected override void OnDestroy()
	{
		foreach ( var item in CornerObjects )
		{
			item.Destroy();
		}
		base.OnDestroy();
	}

	protected override void OnUpdate()
	{
		DrawRope();

		if ( IsProxy ) return;

		MuzzlePoint.Transform.Position = Transform.Position + HookDirection * 15f + mountComponent.Grub.Transform.Rotation.Left * 10f;

		HookDirection = (jointComponent.Body.Transform.Position - Transform.Position).Normal;

		var tr = Scene.Trace.Ray(  Transform.Position - HookDirection, jointComponent.Body.Transform.Position + HookDirection ).WithoutTags("player", "tool", "projectile" ).IgnoreGameObjectHierarchy(GameObject).Run();

		if ( tr.Hit )
		{
			GameObject LastCorner = jointComponent.Body;
			GameObject NewCorner = new GameObject(true, "Corner" );
			NewCorner.Transform.Position = tr.HitPosition + tr.Normal;
			CornerObjects.Add( NewCorner );
			jointComponent.Body = NewCorner;
			//ropeLength -= Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position )*0.8f;
			jointComponent.MaxLength = ropeLength;
		}

		if ( CornerObjects.Count > 1 )
		{
			var tr2 = Scene.Trace.Ray( Transform.Position - HookDirection, CornerObjects[CornerObjects.Count - 2].Transform.Position + HookDirection * 2f ).WithoutTags( "player", "tool", "projectile" ).IgnoreGameObjectHierarchy( GameObject ).Run();

			if ( !tr2.Hit )
			{
				GameObject LastCorner = jointComponent.Body;
				GameObject NewCorner = CornerObjects[CornerObjects.Count - 2];
				jointComponent.Body = NewCorner;
				//ropeLength += Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position );
				CornerObjects[CornerObjects.Count - 1].Destroy();
				CornerObjects.RemoveAt( CornerObjects.Count - 1 );
			}
		}
		else
		{
			var tr2 = Scene.Trace.Ray( Transform.Position - HookDirection,  HookObject.Transform.Position + HookDirection * 2f ).WithoutTags( "player", "tool", "projectile" ).IgnoreGameObjectHierarchy( GameObject ).Run();

			if ( !tr2.Hit )
			{
				GameObject LastCorner = jointComponent.Body;
				GameObject NewCorner = HookObject;
				jointComponent.Body = NewCorner;
				//ropeLength += Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position );
				if ( CornerObjects.Count > 0 )
				{
					CornerObjects[CornerObjects.Count - 1].Destroy();
					CornerObjects.RemoveAt( CornerObjects.Count - 1 );
				}
			}
		}

		jointComponent.MaxLength = ropeLength;

		ropeLength -= Input.AnalogMove.x * Time.Delta * 100f;

		ropeLength = ropeLength.Clamp( 20f, 10000f );

		Vector3 leftDirection = Vector3.Cross( HookDirection, Vector3.Up ).Normal;

		Components.Get<Rigidbody>().Velocity += Vector3.Forward * Input.AnalogMove.y * -10f;

		if ( Input.Pressed( "jump" ) )
		{
			Components.Get<Mountable>().Dismount();
		}
	}

	public void DrawRope()
	{
		if ( CornerObjects.Count > 1 )
		{
			RopeRenderer.Points.Clear();
			RopeRenderer.Points.Add( HookObject );
			RopeRenderer.Points.AddRange(CornerObjects);
			RopeRenderer.Points.Add( MuzzlePoint );
		}
		else
		{
			if ( CornerObjects.Count == 1 )
			{
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( CornerObjects[0] );
				RopeRenderer.Points.Add( MuzzlePoint );
			}
			else
			{
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( MuzzlePoint );
			}
		}
	}
}
