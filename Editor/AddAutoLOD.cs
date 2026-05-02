using Sandbox;
using System.IO;
using System.Linq;
using System.Text;

namespace Editor;

internal static class AddAutoLOD
{
	[Event( "asset.contextmenu", Priority = 60 )]
	public static void OnAssetContext( AssetContextMenu e )
	{
		if ( !e.SelectedList.All( x => x.AbsolutePath.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) ) )
			return;

		e.Menu.AddOption( "Add Automatic LODs", "layers", action: () =>
		{
			foreach ( var entry in e.SelectedList )
				AddLODsToVmdl( entry.AbsolutePath );
		} );
	}

	private static string BuildMeshesArray( string content )
	{
		var names = new System.Collections.Generic.List<string>();
		int unnamedCount = 0;
		int searchFrom = 0;
		const string meshToken = "_class = \"RenderMeshFile\"";

		while ( true )
		{
			int meshIdx = content.IndexOf( meshToken, searchFrom, System.StringComparison.Ordinal );
			if ( meshIdx < 0 ) break;
			searchFrom = meshIdx + meshToken.Length;

			// Look for a name = "..." field before the next _class = or closing }
			int blockEnd = content.IndexOf( "_class =", searchFrom, System.StringComparison.Ordinal );
			if ( blockEnd < 0 ) blockEnd = content.Length;

			int nameIdx = content.IndexOf( "\tname = \"", searchFrom, System.StringComparison.Ordinal );
			if ( nameIdx >= 0 && nameIdx < blockEnd )
			{
				int nameStart = content.IndexOf( '"', nameIdx ) + 1;
				int nameEnd = content.IndexOf( '"', nameStart );
				names.Add( content.Substring( nameStart, nameEnd - nameStart ) );
			}
			else
			{
				names.Add( $"unnamed_{++unnamedCount}" );
			}
		}

		if ( names.Count == 0 )
			names.Add( "unnamed_1" );

		var sb = new StringBuilder();
		sb.Append( "\t\t\t\t\t\t[\n" );
		foreach ( var name in names )
			sb.Append( $"\t\t\t\t\t\t\t\"{name}\",\n" );
		sb.Append( "\t\t\t\t\t\t]" );
		return sb.ToString();
	}

	private static string BuildLodGroupListBlock( string meshesArray )
	{
		string LodGroup( float threshold, int mode, double reduction, bool permissiveSimplification, bool protectUvSeams ) =>
			"\t\t\t\t\t{\n" +
			"\t\t\t\t\t\t_class = \"LODGroup\"\n" +
			$"\t\t\t\t\t\tswitch_threshold = {threshold.ToString( "0.0##", System.Globalization.CultureInfo.InvariantCulture )}\n" +
			$"\t\t\t\t\t\tauto_simplify_mode = {mode}\n" +
			$"\t\t\t\t\t\tauto_reduction = {reduction.ToString( "0.0###", System.Globalization.CultureInfo.InvariantCulture )}\n" +
			"\t\t\t\t\t\tauto_max_error = 0.0\n" +
			"\t\t\t\t\t\tauto_lock_border_vertices = true\n" +
			$"\t\t\t\t\t\tauto_permissive_simplification = {( permissiveSimplification ? "true" : "false" )}\n" +
			$"\t\t\t\t\t\tauto_protect_uv_seams = {( protectUvSeams ? "true" : "false" )}\n" +
			"\t\t\t\t\t\tauto_regularize = 1\n" +
			"\t\t\t\t\t\tauto_prune_isolated_components = false\n" +
			"\t\t\t\t\t\tmeshes = \n" +
			$"{meshesArray}\n" +
			"\t\t\t\t\t},\n";

		return
			"\t\t\t{\n" +
			"\t\t\t\t_class = \"LODGroupList\"\n" +
			"\t\t\t\tchildren = \n" +
			"\t\t\t\t[\n" +
				LodGroup( 0.0f,  0, 0.5, false, true  ) + //LOD0
				LodGroup( 10.0f, 1, 0.7, false, true  ) + //LOD1 - 10 distance, non-permissive simplification, protection of UV seams
				LodGroup( 20.0f, 1, 0.6, false, true  ) + //LOD2 - 20 distance, non-permissive simplification, protection of UV seams
				LodGroup( 40.0f, 2, 0.1, true,  true ) + //LOD3 - 40 distance, aggressive simplification, protection of UV seams
				LodGroup( 60.0f, 1, 0.5, true, false ) + //LOD4 - 60 distance, aggressive simplification, no protection of UV seams
			"\t\t\t\t]\n" +
			"\t\t\t},\n";
	}

	private static void AddLODsToVmdl( string absolutePath )
	{
		string content;

		try
		{
			content = File.ReadAllText( absolutePath ).Replace( "\r\n", "\n" );
		}
		catch ( System.Exception ex )
		{
			Log.Error( $"AddAutoLOD: Failed to read {absolutePath}: {ex.Message}" );
			return;
		}

		if ( content.Contains( "\"LODGroupList\"" ) )
		{
			Log.Warning( $"AddAutoLOD: {Path.GetFileName( absolutePath )} already has a LODGroupList, skipping." );
			return;
		}

		// The rootNode children array closes just before the model_archetype property.
		// We find that boundary and inject the LODGroupList block before the closing bracket.
		const string marker = "\n\t\t]\n\t\tmodel_archetype";
		int insertIndex = content.IndexOf( marker );

		if ( insertIndex < 0 )
		{
			Log.Error( $"AddAutoLOD: Could not find insertion point in {Path.GetFileName( absolutePath )}. Is this a standard modeldoc file?" );
			return;
		}

		var lodBlock = BuildLodGroupListBlock( BuildMeshesArray( content ) );
		var sb = new StringBuilder( content.Length + lodBlock.Length + 16 );
		sb.Append( content, 0, insertIndex );
		sb.Append( '\n' );
		sb.Append( lodBlock );
		sb.Append( "\t\t]" );
		sb.Append( content, insertIndex + "\n\t\t]".Length, content.Length - insertIndex - "\n\t\t]".Length );

		try
		{
			File.WriteAllText( absolutePath, sb.ToString() );
			Log.Info( $"AddAutoLOD: Successfully added LOD groups to {Path.GetFileName( absolutePath )}." );
		}
		catch ( System.Exception ex )
		{
			Log.Error( $"AddAutoLOD: Failed to write {absolutePath}: {ex.Message}" );
		}
	}
}
