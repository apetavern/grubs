using Grubs.Pawn;
using Grubs.UI.Components;

namespace Grubs.Equipment.Tools;

[Title( "Grubs - Teleport Tool" ), Category( "Equipment" )]
public class TeleportTool : Tool
{
	/*
	 * Cursor Properties
	 */
	[Property] public required SkinnedModelRenderer CursorModel { get; set; } // The model of the grub

	protected override void OnStart()
	{
		base.OnStart();

		if ( !CursorModel.IsValid() )
			return;
		
		CursorModel.GameObject.SetParent( Scene );
		CursorModel.GameObject.Enabled = Equipment.Deployed;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy || !Equipment.IsValid() )
			return;

		Cursor.Enabled( "clicktool", Equipment.Deployed );
		
		if ( CursorModel.IsValid() )
			CursorModel.GameObject.Enabled = Equipment.Deployed;

		if ( !Equipment.Deployed )
			return;

		if ( !Equipment.Grub.IsValid() )
			return;

		var player = Equipment.Grub.Owner;
		if ( !player.IsValid() )
			return;

		if ( CursorModel.IsValid() )
		{
			CursorModel.WorldPosition = player.MousePosition;
			var isValidPlacement = CheckValidPlacement();
			CursorModel.Tint = isValidPlacement ? Color.Green : Color.Red;
		}

		if ( Input.UsingController )
			GrubFollowCamera.Local?.PanCamera();
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
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return false;

		var grub = Equipment.Grub;

		var trLocation = Scene.Trace.Box( grub.CharacterController.BoundingBox, grub.Owner.MousePosition, grub.Owner.MousePosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var inTerrain = terrain.PointInside( trLocation.EndPosition );
		var exceedsTerrainHeight = trLocation.EndPosition.z >= terrain.WorldTextureHeight + 64f;

		return !trLocation.Hit && !inTerrain && !exceedsTerrainHeight;
	}

	protected override void FireImmediate()
	{
		if ( !Equipment.Deployed )
			return;

		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return;

		if ( !CheckValidPlacement() )
			return;

		var grub = Equipment.Grub;
		var grubPosition = grub.Owner.MousePosition;
		grub.WorldPosition = grubPosition;
		TeleportEffects( grubPosition );

		base.FireFinished();
	}

	[Rpc.Broadcast]
	public void TeleportEffects( Vector3 position )
	{
		Sound.Play( UseSound, position );
	}
}
