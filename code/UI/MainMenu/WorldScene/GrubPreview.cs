namespace Grubs.UI;

public class GrubPreview : ScenePanel
{
	public SceneModel Grub { get; private set; }
	private readonly List<SceneModel> _sceneClothing = new();
	private int _lastSelectedCosmeticIndex;

	public GrubPreview( SceneWorld world )
	{
		var grub = Model.Load( "models/citizenworm.vmdl" );
		Grub = new SceneModel( world, grub, Transform.Zero );
		World = world;
	}

	public override void Tick()
	{
		foreach ( var model in _sceneClothing )
		{
			model.Update( Time.Delta );
		}

		Grub.Update( Time.Delta );
		Grub.SetAnimParameter( "grounded", true );
		Grub.SetAnimParameter( "incline", 0f );

		if ( Game.LocalPawn is not Player player )
			return;

		var hasChangedCosmetic = player.SelectedCosmeticIndex != _lastSelectedCosmeticIndex;
		if ( !hasChangedCosmetic )
			return;

		_lastSelectedCosmeticIndex = player.SelectedCosmeticIndex;
		for ( int i = _sceneClothing.Count - 1; i >= 0; i-- )
		{
			var clothing = _sceneClothing[i];
			_sceneClothing.Remove( clothing );
			clothing?.Delete();
		}

		var clothingContainer = new ClothingContainer();
		clothingContainer.Deserialize( player.AvatarClothingData );

		Material skinoverride = null;
		Material eyeoverride = null;

		SceneModel skeleton = null;

		for ( int i = 0; i < clothingContainer.Clothing.Count; i++ )
		{
			var item = clothingContainer.Clothing[i];
			if ( item.Category == Clothing.ClothingCategory.Skin )
			{
				if ( item.Model != null )
				{
					if ( item.ResourceName.ToLower().Contains( "skel" ) )
					{
						skeleton = new SceneModel( Grub.World, "models/cosmetics/skeleton/skeleton_grub.vmdl", Grub.Transform );

						_sceneClothing.Add( skeleton );

						Grub.AddChild( "clothing", skeleton );
						Grub.SetBodyGroup( "show", 1 );
					}

					clothingContainer.Clothing.Remove( item );

					var materials = Model.Load( item.Model ).Materials;

					foreach ( var mat in materials )
					{
						if ( mat.Name.Contains( "eyes" ) )
						{
							eyeoverride = mat;
						}

						if ( mat.Name.Contains( "_skin" ) )
						{
							skinoverride = mat;
						}
					}
				}
			}
		}

		if ( player.HasCosmeticSelected )
			clothingContainer.Toggle( Player.CosmeticPresets[player.SelectedCosmeticIndex] );

		_sceneClothing.AddRange( clothingContainer.DressSceneObject( Grub ) );

		if ( skinoverride != null )
		{
			Grub.SetMaterialOverride( skinoverride, "skin" );
			Grub.SetMaterialOverride( eyeoverride, "eyes" );

			if ( skeleton != null )
			{
				skeleton.SetMaterialOverride( skinoverride );
			}
		}
	}
}
