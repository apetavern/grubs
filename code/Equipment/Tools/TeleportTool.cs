using Grubs.UI.Components;

namespace Grubs.Equipment.Tools;

[Title( "Grubs - Teleport Tool" ), Category( "Equipment" )]
public class TeleportTool : Tool
{
	/*
	 * Cursor Properties
	 */
	[Property] public float CursorRange { get; set; } = 100f; // How far from the grub can the tool target
	[Property] public required SkinnedModelRenderer CursorModel { get; set; } // The model of the grub
	[Property, ResourceType( "sound" )] private string TeleportSound { get; set; } = "";

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		Cursor.Enabled( "clicktool", Equipment.Deployed );
		CursorModel.Enabled = Equipment.Deployed;

		if ( !Equipment.Deployed )
			return;

		if ( Equipment.Grub == null )
			return;

		var player = Equipment.Grub.Player;
		CursorModel.Transform.Position = player.MousePosition;
		var isValidPlacement = CheckValidPlacement();
		CursorModel.Tint = isValidPlacement ? Color.Green : Color.Red;
	}

	private bool CheckValidPlacement()
	{
		if ( Equipment.Grub is not { } grub )
			return false;

		if ( grub.Player.MousePosition.Distance( grub.Transform.Position ) > CursorRange )
			return false;

		var trLocation = Scene.Trace.Box( grub.CharacterController.BoundingBox, grub.Player.MousePosition, grub.Player.MousePosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var trTerrain = Scene.Trace.Ray( trLocation.EndPosition, trLocation.EndPosition + Vector3.Right * 64f )
			.WithoutTags( "solid" )
			.Size( 1f )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var exceedsTerrainHeight = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture && trLocation.EndPosition.z >= terrain.WorldTextureHeight - 64f;

		return !trLocation.Hit && !trTerrain.Hit && !exceedsTerrainHeight;
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

		grub.Transform.Position = grub.Player.MousePosition;
		Sound.Play( TeleportSound );
	}
}
