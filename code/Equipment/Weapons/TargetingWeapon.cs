using Sandbox;
using Grubs.UI.Components;
using Grubs.Pawn;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Targeting Weapon" ), Category( "Equipment" )]
public sealed class TargetingWeapon : Weapon
{
	[Property] public ModelRenderer CursorModel { get; set; }
	public Vector3 ProjectileTarget { get; set; }
	[Property] public FiringType SecondaryFiringType { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		CursorModel.GameObject.SetParent( Scene );
		CursorModel.GameObject.Enabled = Equipment.Deployed;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		Cursor.Enabled( "clicktool", Equipment.Deployed && ProjectileTarget == Vector3.Zero );
		CursorModel.GameObject.Enabled = Equipment.Deployed;
	}

	protected override void HandleComplexFiringInput()
	{
		if ( IsProxy )
			return;


		if ( !Equipment.Deployed )
			return;

		if ( Equipment.Grub == null )
			return;

		var player = Equipment.Grub.Player;
		if ( ProjectileTarget == Vector3.Zero )
		{
			CursorModel.Transform.Position = player.MousePosition;
		}
		var isValidPlacement = CheckValidPlacement();
		CursorModel.Tint = isValidPlacement ? Color.Green : Color.Red;

		if ( Input.UsingController )
		{
			IsFiring = true;
			GrubFollowCamera.Local.PanCamera();
		}

		if ( isValidPlacement && Input.Pressed( "fire" ) )
		{
			ProjectileTarget = CursorModel.Transform.Position;
		}

		if ( Input.Released( "fire" ) && ProjectileTarget != Vector3.Zero )
		{
			IsFiring = false;
			FiringType = SecondaryFiringType;
		}
	}

	public void ResetParameters()
	{
		ProjectileTarget = Vector3.Zero;
		CursorModel.GameObject.Enabled = false;
		FiringType = FiringType.Complex;
	}

	private bool CheckValidPlacement()
	{
		if ( Equipment.Grub is not { } grub )
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
		var maxHeight = GrubsConfig.WorldTerrainType is GrubsConfig.TerrainType.Texture ? terrain.WorldTextureHeight : GrubsConfig.TerrainHeight;
		var exceedsTerrainHeight = trLocation.EndPosition.z >= maxHeight - 64f;

		return !trLocation.Hit && !trTerrain.Hit && !exceedsTerrainHeight;
	}
}
