using Grubs.Pawn;
using Grubs.UI.Components;

namespace Grubs.Equipment.Weapons;

[Title( "Grubs - Targeting Weapon" ), Category( "Equipment" )]
public sealed class TargetingWeapon : Weapon
{
	[Property] public ModelRenderer CursorModel { get; set; }
	[Property] public bool CanTargetTerrain { get; set; } = false;
	public Vector3 ProjectileTarget { get; set; }
	public Vector3 Direction { get; set; } = Vector3.Zero;
	[Property] public FiringType SecondaryFiringType { get; set; }
	[Property] public bool Directional { get; set; }

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

		Cursor.Enabled( "clicktool", Equipment.Deployed && ProjectileTarget == Vector3.Zero && CursorModel is null );
		
		if ( CursorModel.IsValid() )
			CursorModel.GameObject.Enabled = Equipment.Deployed;
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

	protected override void HandleComplexFiringInput()
	{
		if ( IsProxy )
			return;
		
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() || !CursorModel.IsValid() )
			return;

		if ( !Equipment.Deployed )
			return;

		var player = Equipment.Grub.Owner;
		if ( !player.IsValid() )
			return;
		
		if ( ProjectileTarget == Vector3.Zero )
		{
			CursorModel.WorldPosition = player.MousePosition.WithY( 480 );
		}

		var isValidPlacement = CheckValidPlacement();
		CursorModel.Tint = isValidPlacement ? Color.Green : Color.Red;

		if ( Input.UsingController )
			GrubFollowCamera.Local.PanCamera();

		if ( Directional )
		{
			if ( Direction == Vector3.Zero )
			{
				Direction = Vector3.Forward;
			}

			IsFiring = true;

			if ( Input.AnalogMove.y > 0.5f )
			{
				Direction = Vector3.Backward;
			}

			if ( Input.AnalogMove.y < -0.5f )
			{
				Direction = Vector3.Forward;
			}

			CursorModel.SetBodyGroup( "arrow_direction", Direction == Vector3.Forward ? 1 : 0 );
		}

		if ( isValidPlacement && Input.Pressed( "fire" ) )
		{
			ProjectileTarget = CursorModel.WorldPosition.WithY( 512f );
		}

		if ( Input.Released( "fire" ) && ProjectileTarget != Vector3.Zero )
		{
			GrubFollowCamera.Local.AutomaticRefocus = true;

			switch ( SecondaryFiringType )
			{
				case FiringType.Instant:
					IsFiring = true;

					if ( OnFire is not null )
						OnFire.Invoke( 100 );
					else
						FireImmediate();

					TimeSinceLastUsed = 0;

					if ( Input.UsingController ) Input.TriggerHaptics( 0, 0.25f, rightTrigger: 0.25f );
					break;
				default:
					IsFiring = false;
					FiringType = SecondaryFiringType;
					if ( WeaponInfoPanel is not null )
					{
						WeaponInfoPanel.Inputs = new Dictionary<string, string>()
						{
							{ "fire", "Fire (Hold)" }
						};
					}
					break;
			}
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
		if ( !Equipment.IsValid() || !Equipment.Grub.IsValid() )
			return false;

		var grub = Equipment.Grub;

		var trLocation = Scene.Trace.Box( grub.CharacterController.BoundingBox, CursorModel.WorldPosition, CursorModel.WorldPosition )
			.IgnoreGameObject( GameObject )
			.Run();

		var trTerrain = Scene.Trace.Ray( trLocation.EndPosition, trLocation.EndPosition + Vector3.Right * 64f )
			.WithoutTags( "solid" )
			.Size( 1f )
			.IgnoreGameObject( GameObject )
			.Run();

		var terrain = Terrain.GrubsTerrain.Instance;
		var exceedsTerrainHeight = trLocation.EndPosition.z >= terrain.WorldTextureHeight + 64f;

		if ( !CanTargetTerrain && trTerrain.Hit )
			return false;

		return (CanTargetTerrain && trLocation.Hit && trLocation.GameObject.Tags.Contains( "terrain" )) || (!trLocation.Hit && !exceedsTerrainHeight);
	}
}
