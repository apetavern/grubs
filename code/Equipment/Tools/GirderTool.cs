using Grubs.Pawn;
using Grubs.UI.Components;

namespace Grubs.Equipment.Tools;

[Title( "Grubs - Girder Tool" ), Category( "Equipment" )]
public sealed class GirderTool : Tool
{
	/*
	 * Cursor Properties
	 */
	[Property] private float CursorRange { get; set; } = 100f;

	[Property] public required ModelRenderer CursorVisual { get; set; }
	[Property] public required ModelCollider CursorCollider { get; set; }

	[Property] private GameObject GirderPrefab { get; set; }

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

		var player = Equipment.Grub.Owner;
		CursorVisual.WorldPosition = player.MousePosition;
		CursorVisual.WorldRotation *= Rotation.FromPitch( (Input.UsingController ? Input.GetAnalog( InputAnalog.LeftStickY ) : Input.MouseWheel.y * 10f) );

		if ( Input.UsingController )
			GrubFollowCamera.Local?.PanCamera();

		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.Distance = 1024f;
		var isValidPlacement = CheckValidPlacement();
		CursorVisual.Tint = (isValidPlacement ? Color.Green : Color.Red).WithAlpha( 0.75f );
	}

	public override void OnDeploy()
	{
		base.OnDeploy();

		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.AutomaticRefocus = false;
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.AutomaticRefocus = true;
	}

	private bool CheckValidPlacement()
	{
		if ( !Equipment.Grub.IsValid() )
			return false;

		var grub = Equipment.Grub;

		if ( grub.Owner.MousePosition.Distance( grub.WorldPosition ) > CursorRange )
			return false;

		var trLocation = Scene.Trace.Body( CursorCollider.KeyframeBody, grub.Owner.MousePosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var exceedsTerrainHeight = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture && trLocation.EndPosition.z >= terrain.WorldTextureHeight + 64f;

		return !trLocation.Hit && !exceedsTerrainHeight;
	}

	protected override void FireImmediate()
	{
		if ( !Equipment.IsValid() || !Equipment.Deployed )
			return;

		var valid = CheckValidPlacement();
		if ( !valid )
			return;

		var girder = GirderPrefab.Clone( CursorVisual.Transform.World );
		girder.NetworkSpawn();

		Equipment.Deployed = false;
		IsFiring = false;

		Sound.Play( UseSound );

		FireFinished();
	}
}
