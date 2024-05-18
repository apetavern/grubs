using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor;
using Sandbox;

namespace QuickSwitcher;

[Flags]
public enum OptionType
{
	Asset,
	Action,

	All
}

public static class OptionTypeExtension
{
	public static Color GetColor( this OptionType type )
	{
		return type switch
		{
			OptionType.Asset => Theme.White,
			OptionType.Action => Theme.Primary,
			_ => Color.White
		};
	}
}

public record Option( OptionType Type, string Name, string ActionText, Pixmap Icon );

public record AssetOption( OptionType Type, string Name, string ActionText, Pixmap Icon, Asset asset )
	: Option( Type, Name, ActionText, Icon );

public record ActionOption(
	OptionType Type,
	string Name,
	string ActionText,
	Pixmap Icon,
	Action action )
	: Option( Type, Name, ActionText, Icon )
{
	public static List<ActionOption> All()
	{
		List<ActionOption> options = new();

		foreach ( var entry in CreateAsset.BuiltIn )
		{
			var asset = AssetType.Find( entry.Name );

			if ( entry.Name == "Particles" )
			{
				asset = AssetType.ParticleSystem;
			}

			if ( asset is null )
			{
				Log.Warning( $"{entry.Name} is null" );
				continue;
			}

			options.Add(
				new ActionOption( OptionType.Action, $"New {asset.FriendlyName}..", "Action", asset.Icon64, () =>
				{
					var extension = Path.GetExtension( entry.Default ).Trim( '.' );

					var fd = new FileDialog( null );
					fd.Title = $"Create {entry.Name}";
					fd.Directory = Project.Current.RootDirectory.FullName;
					fd.DefaultSuffix = $".{extension}";
					fd.SelectFile( $"untitled.{extension}" );
					fd.SetFindFile();
					fd.SetModeSave();
					fd.SetNameFilter( $"{entry.Name} (*.{extension})" );

					if ( !fd.Execute() )
						return;

					CreateAsset.Create( entry, fd.SelectedFile );
				} ) );
		}

		foreach ( var gameResource in EditorTypeLibrary.GetAttributes<GameResourceAttribute>().OrderBy( x => x.Name ) )
		{
			void CreateResource( string s )
			{
				var asset = AssetSystem.CreateResource( gameResource.Extension, s );
				MainAssetBrowser.Instance?.UpdateAssetList();
				MainAssetBrowser.Instance?.FocusOnAsset( asset );
				EditorUtility.InspectorObject = asset;
			}

			var asset = AssetType.FromType( gameResource.TargetType );

			options.Add( new ActionOption(
				OptionType.Action,
				$"New {asset.FriendlyName}..",
				"Action",
				asset.Icon64,
				() =>
				{
					var fd = new FileDialog( null );
					fd.Title = $"Create {gameResource.Name}";
					fd.Directory = Project.Current.RootDirectory.FullName;
					fd.DefaultSuffix = $".{gameResource.Extension}";
					fd.SelectFile( $"untitled.{gameResource.Extension}" );
					fd.SetFindFile();
					fd.SetModeSave();
					fd.SetNameFilter( $"{gameResource.Name} (*.{gameResource.Extension})" );

					if ( !fd.Execute() )
						return;

					CreateResource( fd.SelectedFile );
				}
			) );
		}

		return options;
	}
}
