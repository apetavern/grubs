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

	protected override void OnAwake()
	{
		jointComponent = Components.Get<SpringJoint>();
		jointComponent.MaxLength = Vector3.DistanceBetween( Transform.Position, HookObject.Transform.Position );
		jointComponent.Body = HookObject;
		ropeLength = jointComponent.MaxLength;
	}

	[Property] float ropeLength {  get; set; }

	protected override void OnUpdate()
	{
		HookDirection = (jointComponent.Body.Transform.Position - Transform.Position).Normal;

		var tr = Scene.Trace.Ray(  Transform.Position, jointComponent.Body.Transform.Position + HookDirection ).IgnoreGameObjectHierarchy(GameObject).Run();

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
			var tr2 = Scene.Trace.Ray( Transform.Position, CornerObjects[CornerObjects.Count - 2].Transform.Position + HookDirection * 2f ).IgnoreGameObjectHierarchy( GameObject ).Run();

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
			var tr2 = Scene.Trace.Ray( Transform.Position,  HookObject.Transform.Position + HookDirection * 2f ).IgnoreGameObjectHierarchy( GameObject ).Run();

			if ( !tr2.Hit )
			{
				GameObject LastCorner = jointComponent.Body;
				GameObject NewCorner = HookObject;
				jointComponent.Body = NewCorner;
				//ropeLength += Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position );
				CornerObjects[CornerObjects.Count - 1].Destroy();
				CornerObjects.RemoveAt( CornerObjects.Count - 1 );
			}
		}

		jointComponent.MaxLength = ropeLength;

		ropeLength -= Input.AnalogMove.x * Time.Delta * 100f;

		ropeLength = ropeLength.Clamp( 100f, 10000f );

		Vector3 leftDirection = Vector3.Cross( HookDirection, Vector3.Up ).Normal;

		Components.Get<Rigidbody>().Velocity += leftDirection * Input.AnalogMove.y * -10f;

		DrawRope();
	}

	public void DrawRope()
	{
		if ( CornerObjects.Count > 1 )
		{
			RopeRenderer.Points.Clear();
			RopeRenderer.Points.Add( HookObject );
			RopeRenderer.Points.AddRange(CornerObjects);
			RopeRenderer.Points.Add( GameObject );
		}
		else
		{
			if ( CornerObjects.Count == 1 )
			{
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( CornerObjects[0] );
				RopeRenderer.Points.Add( GameObject );
			}
			else
			{
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( GameObject );
			}
		}
	}
}
