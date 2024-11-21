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

	[Property] public GameObject GirderPrefab { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( !CursorVisual.IsValid() )
			return;
		
		CursorVisual.GameObject.SetParent( Scene );
		CursorVisual.GameObject.Enabled = Equipment.Deployed;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy || !Equipment.IsValid() || !Equipment.Grub.IsValid() )
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
		CursorVisual.WorldPosition = player.MousePosition;
		CursorVisual.WorldRotation *= Rotation.FromPitch( (Input.UsingController ? Input.GetAnalog( InputAnalog.LeftStickY ) : Input.MouseWheel.y * 10f) );

		if ( Input.UsingController )
			GrubFollowCamera.Local.PanCamera();

		GrubFollowCamera.Local.Distance = 1024f;
		var isValidPlacement = CheckValidPlacement();
		CursorVisual.Tint = (isValidPlacement ? Color.Green : Color.Red).WithAlpha( 0.75f );
	}

	public override void OnDeploy()
	{
		base.OnDeploy();

		GrubFollowCamera.Local.AutomaticRefocus = false;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		GrubFollowCamera.Local.AutomaticRefocus = true;
	}

	private bool CheckValidPlacement()
	{
		if ( !Equipment.Grub.IsValid() )
			return false;

		var grub = Equipment.Grub;

		if ( grub.Player.MousePosition.Distance( grub.WorldPosition ) > CursorRange )
			return false;

		var trLocation = Scene.Trace.Body( CursorCollider.KeyframeBody, grub.Player.MousePosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var exceedsTerrainHeight = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture && trLocation.EndPosition.z >= terrain.WorldTextureHeight + 64f;

		return !trLocation.Hit && !exceedsTerrainHeight;
	}

	protected override void FireImmediate()
	{
		if ( !Equipment.IsValid() )
			return;
		
		if ( !Equipment.Deployed )
			return;

		var valid = CheckValidPlacement();
		if ( !valid )
			return;

		var girder = GirderPrefab.Clone( CursorVisual.Transform.World );
		girder.NetworkSpawn();

		IsFiring = false;

		Sound.Play( UseSound );

		base.FireFinished();
	}
}
