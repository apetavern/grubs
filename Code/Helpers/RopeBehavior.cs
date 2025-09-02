using Grubs.Common;
using Grubs.Systems.Pawn.Grubs;

namespace Grubs.Helpers;

[Title("Grubs - Rope Behaviour"), Category("Grubs/Helpers")]
public sealed class RopeBehavior : Component
{
	[Sync] private List<Vector3> CornerObjects { get; set; } = new();
	
	// The LineRenderer for the rope.
	[Property] private VecLineRenderer RopeRenderer { get; set; }
	
	// The spring joint, between the hook tip and the mountable.
	[Property] private SpringJoint SpringJoint { get; set; }
	
	// The hook tip object.
	[Property] public GameObject HookObject { get; set; }
	
	// The mountable component.
	[Property] private Mountable Mountable { get; set; }
	
	// The rigidbody for the mountable object.
	[Property] private Rigidbody Rigidbody { get; set; }
	
	// The grub who fired this rope.
	public Grub Grub { get; set; }
	
	// The direction between the grub and the hook tip.
	public Vector3 HookDirection { get; private set; }
	
	[Property] private float RopeLength { get; set; }

	private GameObject MuzzlePoint { get; set; }
	private GameObject AttachPoint { get; set; }

	private const float RopeClimbSpeed = 256f;
	private const float RopeSwingSpeed = 6.5f;
	private const float GrubMountPositionOffsetY = 15f;
	private const float GrubMountPositionOffsetX = 10f;

	protected override void OnEnabled()
	{
		if ( IsProxy )
			return;
		
		Log.Info( "Starting RopeBehavior." );
		
		// Setup the initial length of the spring joint - the distance between the hook and the grub.
		var initialLength = Vector3.DistanceBetween( HookObject.WorldPosition, Grub.WorldPosition );
		RopeLength = initialLength;
		
		SpringJoint.RestLength = initialLength;

		// Use manual local frames for the spring joint so that the spring starts "expanded" to some degree
		// between the hook tip and the mountable.
		SpringJoint.Attachment = Joint.AttachmentMode.LocalFrames;
		SpringJoint.LocalFrame1 = Mountable.GetComponent<SphereCollider>().LocalTransform;
		SpringJoint.LocalFrame2 = GetComponent<SphereCollider>().LocalTransform;
		
		// Set the mountable to the Grub's location, and mount the grub on it.
		Mountable.WorldPosition = Grub.WorldPosition;
		Mountable.Mount( Grub );

		// Enable the rigidbody.
		var body = GetComponent<Rigidbody>( true );
		if ( body.IsValid() )
			body.Enabled = true;
		
		AttachPoint = new GameObject();
		MuzzlePoint = new GameObject( true, "Muzzle Point" );
	}

	protected override void OnDestroy()
	{
		AttachPoint?.Destroy();
		MuzzlePoint?.Destroy();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		
		DrawRope();
		
		HookDirection = (SpringJoint.Body.WorldPosition - WorldPosition).Normal;
		RopeLength -= Input.AnalogMove.x * Time.Delta * RopeClimbSpeed;
		RopeLength = RopeLength.Clamp( SpringJoint.MinLength, 10000 );
		
		if ( Rigidbody.IsValid() )
			Rigidbody.Velocity += Vector3.Forward * Input.AnalogMove.y * -RopeSwingSpeed;
		
		if ( SpringJoint.IsValid() )
			SpringJoint.RestLength = RopeLength;

		// It's possible the mountable was destroyed before this has been cleaned up.
		if ( !Mountable.IsValid() || !Grub.IsValid() )
			return;
		
		Grub.WorldPosition = Mountable.WorldPosition;
				
		if ( MuzzlePoint is null )
			return;
		
		MuzzlePoint.WorldPosition = WorldPosition + HookDirection * GrubMountPositionOffsetY 
		                                          + Grub.WorldRotation.Left * GrubMountPositionOffsetX;
		
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
			SpringJoint.RestLength = RopeLength;
		}
		
		if ( CornerObjects.Count > 1 )
		{
			var tr2 = Scene.Trace.Ray(
					WorldPosition - HookDirection,
					CornerObjects[^2] + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( tr2.Hit ) 
				return;
			
			AttachPoint.WorldPosition = CornerObjects[^2];
			CornerObjects.RemoveAt( CornerObjects.Count - 1 );
		}
		else
		{
			var tr2 = Scene.Trace.Ray(
					WorldPosition - HookDirection,
					HookObject.WorldPosition + HookDirection * 2f )
				.WithoutTags( "player", "tool", "projectile" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			if ( tr2.Hit ) 
				return;
			
			AttachPoint.WorldPosition = HookObject.WorldPosition;
			
			if ( CornerObjects.Count > 0 )
			{
				CornerObjects.RemoveAt( CornerObjects.Count - 1 );
			}
		}
	}

	private void DrawRope()
	{
		if ( !RopeRenderer.IsValid() )
			return;

		switch ( CornerObjects.Count )
		{
			case > 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				RopeRenderer.Points.AddRange( CornerObjects );
				break;
			case 1:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				RopeRenderer.Points.Add( CornerObjects[0] );
				break;
			default:
				RopeRenderer.Points.Clear();
				RopeRenderer.Points.Add( HookObject.WorldPosition );
				break;
		}

		RopeRenderer.Points.Add( MuzzlePoint.WorldPosition );
	}
}
