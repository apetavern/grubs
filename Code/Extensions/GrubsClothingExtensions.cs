using Sandbox.Internal;
using System.Threading;
using Sandbox.Services;

namespace Grubs.Extensions;

public static class GrubsClothingExtensions
{
	public static async Task ApplyAsync( ClothingContainer container, SkinnedModelRenderer body, CancellationToken token, List<Clothing.ClothingCategory> allowedCategories )
	{
		if ( !body.IsValid() )
			return;

		Scene scene = body.Scene;
		ApplyToGrub( container, body );
		bool hasChanges = false;

		var entriesToLoad = container.Clothing.Where( x => x.Clothing == null || x.Clothing.ResourceId == 0 ).ToArray();

		foreach ( var entry in entriesToLoad )
		{
			if ( entry.ItemDefinitionId == 0 )
				continue;

			var itemDef = Inventory.FindDefinition( entry.ItemDefinitionId );
			if ( itemDef == null )
			{
				GlobalSystemNamespace.Log.Warning( $"FindDefinition null : {entry.ItemDefinitionId}" );
				continue;
			}

			// Try loading from resource library first
			var clothing = ResourceLibrary.Get<Clothing>( itemDef.Asset );
			if ( clothing != null )
			{
				if ( allowedCategories.Contains( clothing.Category ) )
				{
					entry.Clothing = clothing;
					hasChanges = true;
				}
				else
				{
					entry.Clothing = null;
				}
				continue;
			}

			// Fallback to cloud loading
			clothing = await Cloud.Load<Clothing>( itemDef.PackageIdent );
			if ( clothing == null )
			{
				GlobalSystemNamespace.Log.Warning( $"Clothing from package was null: {itemDef.PackageIdent}" );
				continue;
			}

			token.ThrowIfCancellationRequested();
			if ( !body.IsValid() )
				return;

			if ( allowedCategories.Contains( clothing.Category ) )
			{
				entry.Clothing = clothing;
				hasChanges = true;
			}
			else
			{
				entry.Clothing = null;
			}
		}

		if ( hasChanges )
		{
			using ( scene.Push() )
			{
				var clothingArray = container.Clothing.ToArray();
				foreach ( var clothing in clothingArray )
				{
					container.Clothing.Add( clothing );
				}

				ApplyToGrub( container, body );
			}
		}
	}

	public static void ApplyToGrub( ClothingContainer container, SkinnedModelRenderer body )
	{
		var clothing = container?.Clothing;
		body.Attributes.Set( "skin_age", container.Age );
		body.Attributes.Set( "skin_tint", container.Tint );

		var validClothingEntries = clothing.Where( x => x?.Clothing != null ).ToList();
		var tagSet = new TagSet();

		// Get skin and eyes material overrides
		var skinMaterial = validClothingEntries
			.Select( x => x.Clothing.SkinMaterial )
			.Where( x => !string.IsNullOrWhiteSpace( x ) )
			.Select( x => Material.Load( x ) )
			.FirstOrDefault();

		var eyesMaterial = validClothingEntries
			.Select( x => x.Clothing.EyesMaterial )
			.Where( x => !string.IsNullOrWhiteSpace( x ) )
			.Select( x => Material.Load( x ) )
			.FirstOrDefault();

		body.SetMaterialOverride( skinMaterial, "skin" );
		body.SetMaterialOverride( eyesMaterial, "eyes" );

		foreach ( var entry in validClothingEntries )
		{
			var clothingItem = entry.Clothing;
			var otherClothing = validClothingEntries.Select( x => x.Clothing ).Except( new[] { clothingItem } );
			var modelPath = clothingItem.GetModel( otherClothing, tagSet );

			if ( string.IsNullOrEmpty( modelPath ) || !string.IsNullOrEmpty( clothingItem.SkinMaterial ) )
				continue;

			var model = Model.Load( modelPath );
			if ( !model.IsValid() || model.IsError )
				continue;

			var clothingObject = new GameObject( enabled: false, $"Clothing - {clothingItem.ResourceName}" )
			{
				Parent = body.GameObject
			};
			clothingObject.Tags.Add( "clothing" );
			clothingObject.Tags.Add( clothingItem.Category.ToString().ToLowerInvariant() );

			var renderer = clothingObject.Components.Create<SkinnedModelRenderer>();
			renderer.Model = model;
			renderer.BoneMergeTarget = body;
			renderer.Attributes.Set( "skin_age", container.Age );
			renderer.Attributes.Set( "skin_tint", container.Tint );
			renderer.SetMaterialOverride( skinMaterial, "skin" );
			renderer.SetMaterialOverride( eyesMaterial, "eyes" );

			if ( !string.IsNullOrEmpty( clothingItem.MaterialGroup ) )
				renderer.MaterialGroup = clothingItem.MaterialGroup;

			if ( clothingItem.AllowTintSelect )
			{
				var tintValue = entry.Tint?.Clamp( 0f, 1f ) ?? clothingItem.TintDefault;
				var tintColor = clothingItem.TintSelection.Evaluate( tintValue );
				renderer.Tint = tintColor;
			}

			clothingObject.Enabled = true;
		}
	}
}
