using Sandbox;

namespace Grubs;

public sealed class RopeBehaviorComponent : Component
{
	[Property] public GameObject HookObject { get; set; }

	[Property] public List<GameObject> CornerObjects { get; set; } = new List<GameObject>();

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
			ropeLength -= Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position )*0.95f;
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
				ropeLength += Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position );
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
				ropeLength += Vector3.DistanceBetween( LastCorner.Transform.Position, NewCorner.Transform.Position );
			}
		}

		jointComponent.MaxLength = ropeLength;

		ropeLength -= Input.AnalogMove.x * Time.Delta * 100f;

		ropeLength = ropeLength.Clamp( 10f, 10000f );

		Components.Get<Rigidbody>().Velocity += Transform.Rotation.Forward * Input.AnalogMove.y * -10f;

		DrawDebugs();
	}

	public void DrawDebugs()
	{
		Gizmo.Draw.LineThickness = 3f;
		if ( CornerObjects.Count > 1 )
		{
			Gizmo.Draw.Line( HookObject.Transform.Position, CornerObjects[0].Transform.Position );
			for ( int i = 0; i < CornerObjects.Count - 1; i++ )
			{
				Gizmo.Draw.Line( CornerObjects[i].Transform.Position, CornerObjects[i + 1].Transform.Position );
			}
			Gizmo.Draw.Line( CornerObjects.Last().Transform.Position, Transform.Position );
		}
		else
		{
			if ( CornerObjects.Count == 1 )
			{
				Gizmo.Draw.Line( CornerObjects[0].Transform.Position, Transform.Position );
				Gizmo.Draw.Line( HookObject.Transform.Position, CornerObjects[0].Transform.Position );
			}
			else
			{
				Gizmo.Draw.Line( HookObject.Transform.Position, Transform.Position );
			}
		}
		Gizmo.Draw.LineThickness = 1f;
	}
}
