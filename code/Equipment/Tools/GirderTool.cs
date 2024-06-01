using Grubs.Pawn;
using Grubs.UI.Components;

namespace Grubs.Equipment.Tools;

[Title( "Grubs - Girder Tool" ), Category( "Equipment" )]
public class GirderTool : Tool
{
	/*
	 * Cursor Properties
	 */
	[Property] public float CursorRange { get; set; } = 100f; // How far from the grub can the tool target

	[Property] public required ModelRenderer CursorVisual { get; set; } // The model of the grub
	[Property] public required ModelCollider CursorCollider { get; set; } // The model of the grub
	[Property, ResourceType( "sound" )] private string PlaceSound { get; set; } = "";

	[Property] public GameObject GirderPrefab { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		CursorVisual.GameObject.SetParent( Scene );
		CursorVisual.GameObject.Enabled = Equipment.Deployed;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		Cursor.Enabled( "clicktool", Equipment.Deployed );
		CursorVisual.Enabled = Equipment.Deployed;
		CursorCollider.Enabled = Equipment.Deployed;

		CursorVisual.GameObject.Enabled = Equipment.Deployed;

		if ( !Equipment.Deployed )
			return;

		if ( Equipment.Grub == null )
			return;

		var player = Equipment.Grub.Player;
		CursorVisual.Transform.Position = player.MousePosition;
		CursorVisual.Transform.Rotation *= Rotation.FromPitch( (Input.UsingController ? Input.GetAnalog( InputAnalog.LeftStickY ) : Input.MouseWheel.y * 10f) );

		if ( Input.UsingController )
		{
			IsFiring = true;
			GrubFollowCamera.Local.PanCamera();
		}

		GrubFollowCamera.Local.Distance = 1024f;
		var isValidPlacement = CheckValidPlacement();
		CursorVisual.Tint = (isValidPlacement ? Color.Green : Color.Red).WithAlpha( 0.75f );
	}

	private bool CheckValidPlacement()
	{
		if ( Equipment.Grub is not { } grub )
			return false;

		if ( grub.Player.MousePosition.Distance( grub.Transform.Position ) > CursorRange )
			return false;

		var trLocation = Scene.Trace.Body( CursorCollider.KeyframeBody, grub.Player.MousePosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var exceedsTerrainHeight = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture && trLocation.EndPosition.z >= terrain.WorldTextureHeight - 64f;

		return !trLocation.Hit && !exceedsTerrainHeight;
	}

	protected override void FireImmediate()
	{
		if ( !Equipment.Deployed )
			return;

		if ( Equipment.Grub is not { } grub )
			return;

		var valid = CheckValidPlacement();
		if ( !valid )
			return;

		var girder = GirderPrefab.Clone( CursorVisual.Transform.World );
		girder.NetworkSpawn();

		IsFiring = false;

		Sound.Play( PlaceSound );

		base.FireFinished();
	}
}
