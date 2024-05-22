using Grubs.UI.Components;

namespace Grubs.Equipment.Weapons;

[Title( "Grub - Click Tool" ), Category( "Equipment" )]
public class ClickToolWeaponComponent : WeaponComponent
{
	/*
	 * Cursor Properties
	 */

	[Property] public float CursorRange { get; set; } = 100f; // How far from the grub can the tool target
	[Property, ResourceType( "jpg" )] public string CursorImage { get; set; } = "";

	private GameObject TestObject { get; set; }
	private SkinnedModelRenderer TestObjectSkin { get; set; }
	protected override void OnStart()
	{
		base.OnStart();

		// DEBUG: Get the debug object
		TestObject = Scene.GetAllObjects(true).First( o => o.Name == "MouseTest" );
		TestObjectSkin = TestObject.Components.Get<SkinnedModelRenderer>();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		Cursor.Enabled( "clicktool", Equipment.Deployed );

		if ( !Equipment.Deployed )
			return;

		if (TestObject != null)
		{
			var traceDistance = ( Scene.Camera.Transform.LocalPosition.y * -1 ) + 512;
			var mouseTrace = Scene.Trace.Ray( Scene.Camera.ScreenPixelToRay( Mouse.Position ), traceDistance )
				.UseHitboxes(true)
				.Run();

			if (mouseTrace.Hit)
			{
				TestObjectSkin.Tint = Color.Red;
			} else
			{
				TestObjectSkin.Tint = Color.White;
			}

			TestObject.Transform.Position = mouseTrace.EndPosition;
		}
	}

	protected override void FireImmediate()
	{
		// Get the mouse position in the world
	}
}
