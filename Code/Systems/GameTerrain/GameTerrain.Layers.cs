namespace Grubs.Terrain;

public partial class GameTerrain
{
	[Property] public List<Material> AvailableMaterials { get; set; } = new();

	[ConCmd( "gr_add_layer" )]
	public static void AddLayerCmd( string matName )
	{
		var material = Local.AvailableMaterials.FirstOrDefault( x => x.Name.Contains( matName ) );
		if ( material is null )
			return;

		var layerDef = new LayerDefinition( matName, material.ResourcePath, material.ShaderName );
		Local.AddLayer( layerDef );
	}
	
	public void AddLayer( LayerDefinition layerDefinition )
	{
		LevelDefinition.Layers.Add( layerDefinition );
		LayerUtility.AddLayer( layerDefinition.LayerId, layerDefinition.GetLayer() );
	}
}
