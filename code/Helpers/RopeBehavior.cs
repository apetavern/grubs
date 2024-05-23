using Grubs.Common;

namespace Grubs.Helpers;

public sealed class RopeBehavior : Component
{
	[Property] public GameObject HookObject { get; set; }
	[Property] public List<GameObject> CornerObjects { get; set; } = new();
	[Property] public LineRenderer RopeRenderer { get; set; }
	[Property] private float RopeLength {  get; set; }

	private GameObject MuzzlePoint { get; set; }
	private Mountable MountComponent { get; set; }
	private SpringJoint JointComponent { get; set; }
	public Vector3 HookDirection { get; private set; }

	protected override void OnAwake()
	{
		JointComponent = Components.Get<SpringJoint>();
		JointComponent.MaxLength = Vector3.DistanceBetween( Transform.Position, HookObject.Transform.Position );
		JointComponent.Body = HookObject;
		RopeLength = JointComponent.MaxLength;

		MountComponent = Components.Get<Mountable>();

		MuzzlePoint = new GameObject( true, "MuzzlePoint" );
	}

	protected override void OnDestroy()
	{
		foreach ( var item in CornerObjects )
			item.Destroy();
		base.OnDestroy();
	}

	protected override void OnUpdate()
	{
		DrawRope();

		if ( IsProxy ) 
			return;

		if ( MuzzlePoint is null || !MountComponent.IsValid() || !MountComponent.Grub.IsValid() )
			return;
		
		MuzzlePoint.Transform.Position = Transform.Position + HookDirection * 15f + MountComponent.Grub.Transform.Rotation.Left * 10f;
		HookDirection = (JointComponent.Body.Transform.Position - Transform.Position).Normal;

		var tr = Scene.Trace.Ray( 
				Transform.Position - HookDirection, 
				JointComponent.Body.Transform.Position + HookDirection )
			.WithoutTags("player", "tool", "projectile" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( tr.Hit )
		{
			var newCorner = new GameObject(true, "Corner" )
			{
				Transform = { Position = tr.HitPosition + tr.Normal }
			};
			CornerObjects.Add( newCorner );
			JointComponent.Body = newCorner;
			JointComponent.MaxLength = RopeLength;
		}

		if ( CornerObjects.Count > 1 )
		{
			var tr2 = Scene.Trace.Ray( 
					Transform.Position - HookDirection, 
					CornerObjects[^2].Transform.Position + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( !tr2.Hit )
			{
				var newCorner = CornerObjects[^2];
				JointComponent.Body = newCorner;
				CornerObjects[^1].Destroy();
				CornerObjects.RemoveAt( CornerObjects.Count - 1 );
			}
		}
		else
		{
			var tr2 = Scene.Trace.Ray( 
					Transform.Position - HookDirection, 
					HookObject.Transform.Position + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( !tr2.Hit )
			{
				var newCorner = HookObject;
				JointComponent.Body = newCorner;
				if ( CornerObjects.Count > 0 )
				{
					CornerObjects[^1].Destroy();
					CornerObjects.RemoveAt( CornerObjects.Count - 1 );
				}
			}
		}

		JointComponent.MaxLength = RopeLength;

		RopeLength -= Input.AnalogMove.x * Time.Delta * 100f;
		RopeLength = RopeLength.Clamp( 20f, 10000f );

		Components.Get<Rigidbody>().Velocity += Vector3.Forward * Input.AnalogMove.y * -6.5f;
	}

	public void DrawRope()
	{
		switch (CornerObjects.Count)
		{
			case > 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.AddRange( CornerObjects );
				RopeRenderer.Points.Add( MuzzlePoint );
				break;
			case 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( CornerObjects[0] );
				RopeRenderer.Points.Add( MuzzlePoint );
				break;
			default:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject );
				RopeRenderer.Points.Add( MuzzlePoint );
				break;
		}
	}
}
