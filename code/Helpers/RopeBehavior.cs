using Grubs.Common;

namespace Grubs.Helpers;

public sealed class RopeBehavior : Component
{
	[Property] public GameObject HookObject { get; set; }
	[Property, Sync] public List<Vector3> CornerObjects { get; set; } = new();
	[Property] public VecLineRenderer RopeRenderer { get; set; }
	[Property] private float RopeLength { get; set; }

	private GameObject MuzzlePoint { get; set; }
	private Mountable MountComponent { get; set; }
	private SpringJoint JointComponent { get; set; }
	public Vector3 HookDirection { get; private set; }

	private GameObject AttachPoint { get; set; }

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		if ( !HookObject.IsValid() )
			return;

		AttachPoint = new GameObject();

		if ( !AttachPoint.IsValid() )
			return;

		JointComponent = Components.Get<SpringJoint>();

		if ( !JointComponent.IsValid() )
			return;
		
		JointComponent.MaxLength = Vector3.DistanceBetween( WorldPosition, HookObject.WorldPosition );
		JointComponent.Body = HookObject;
		RopeLength = JointComponent.MaxLength;

		MountComponent = Components.Get<Mountable>();

		MuzzlePoint = new GameObject( true, "MuzzlePoint" );
	}

	protected override void OnDestroy()
	{
		AttachPoint?.Destroy();
	}

	protected override void OnUpdate()
	{
		DrawRope();

		if ( IsProxy )
			return;

		if ( MuzzlePoint is null || !MountComponent.IsValid() || !MountComponent.Grub.IsValid() )
			return;

		MuzzlePoint.WorldPosition = WorldPosition + HookDirection * 15f + MountComponent.Grub.WorldRotation.Left * 10f;
		HookDirection = (JointComponent.Body.WorldPosition - WorldPosition).Normal;

		var tr = Scene.Trace.Ray(
				WorldPosition - HookDirection,
				AttachPoint.WorldPosition + HookDirection )
			.WithoutTags( "player", "tool", "projectile" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( tr.Hit )
		{
			AttachPoint.WorldPosition = tr.HitPosition + tr.Normal;
			CornerObjects.Add( tr.HitPosition + tr.Normal );
			JointComponent.MaxLength = RopeLength;
		}

		if ( CornerObjects.Count > 1 )
		{
			var tr2 = Scene.Trace.Ray(
					WorldPosition - HookDirection,
					CornerObjects[^2] + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( !tr2.Hit )
			{
				AttachPoint.WorldPosition = CornerObjects[^2];
				CornerObjects.RemoveAt( CornerObjects.Count - 1 );
			}
		}
		else
		{
			var tr2 = Scene.Trace.Ray(
					WorldPosition - HookDirection,
					HookObject.WorldPosition + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( !tr2.Hit )
			{
				AttachPoint.WorldPosition = HookObject.WorldPosition;
				if ( CornerObjects.Count > 0 )
				{
					CornerObjects.RemoveAt( CornerObjects.Count - 1 );
				}
			}
		}

		JointComponent.Body = AttachPoint;

		JointComponent.MaxLength = RopeLength;

		RopeLength -= Input.AnalogMove.x * Time.Delta * 100f;
		RopeLength = RopeLength.Clamp( 20f, 10000f );

		Components.Get<Rigidbody>().Velocity += Vector3.Forward * Input.AnalogMove.y * -6.5f;
	}

	public void DrawRope()
	{
		if ( RopeRenderer is null )
			return;

		switch ( CornerObjects.Count )
		{
			case > 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				RopeRenderer.Points.AddRange( CornerObjects );
				RopeRenderer.Points.Add( MuzzlePoint.WorldPosition );
				break;
			case 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				RopeRenderer.Points.Add( CornerObjects[0] );
				RopeRenderer.Points.Add( MuzzlePoint.WorldPosition );
				break;
			default:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				RopeRenderer.Points.Add( MuzzlePoint.WorldPosition );
				break;
		}
	}
}
