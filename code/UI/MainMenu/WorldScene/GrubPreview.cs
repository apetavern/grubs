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
		if ( !IsVisible || Game.LocalPawn is not Player player )
			return;

		foreach ( var model in _sceneClothing )
		{
			model.Update( Time.Delta );
		}

		Grub.Update( Time.Delta );
		Grub.SetAnimParameter( "grounded", true );
		Grub.SetAnimParameter( "incline", 0f );

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
		if ( player.HasCosmeticSelected )
			clothingContainer.Toggle( new Clothing() { Model = player.CosmeticPresets[player.SelectedCosmeticIndex].Model.ResourcePath } );
		else
			clothingContainer.LoadFromClient( player.Client );

		_sceneClothing.AddRange( clothingContainer.DressSceneObject( Grub ) );
	}
}
