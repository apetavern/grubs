using Grubs.Utils;
using Grubs.Weapons;

namespace Grubs.Player;

[Category( "Grubs" )]
public partial class Worm : AnimatedEntity
{
	[Net, Predicted]
	public WormController Controller { get; set; }

	[Net, Predicted]
	public WormAnimator Animator { get; set; }

	[Net, Predicted]
	public GrubsWeapon ActiveChild { get; set; }

	[Net, Predicted]
	public GrubsWeapon LastActiveChild { get; set; }

	public int TeamNumber => Owner is GrubsPlayer player ? player.TeamNumber : 1;

	public bool IsTurn
	{
		get
		{
			if ( Owner is not GrubsPlayer player )
				return false;
			
			return player.ActiveWorm == this && player.IsTurn;
		}
	}

	public void Spawn( Client cl )
	{
		base.Spawn();

		SetModel( "models/citizenworm.vmdl" );

		Name = Rand.FromArray( GameConfig.WormNames );
		Health = 100;

		Controller = new WormController();
		Animator = new WormAnimator();

		DressFromClient( cl );
		SetHatVisible( true );

	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );

		if ( IsTurn )
			SimulateActiveChild( cl, ActiveChild );

	}

	public virtual void SimulateActiveChild( Client client, GrubsWeapon child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		LastActiveChild.Simulate( client );
	}

	public virtual void OnActiveChildChanged( GrubsWeapon previous, GrubsWeapon next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	public void EquipWeapon( GrubsWeapon weapon )
	{
		ActiveChild = weapon;
	}

	public char GetTeamName()
	{
		var index = TeamNumber - 1;

		return GameConfig.TeamNames[index];
	}

	public void DressFromClient( Client cl )
	{
		var clothes = new ClothingContainer();
		clothes.LoadFromClient( cl );

		// Skin tone
		var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizenworm.vmdl" );
		SetMaterialGroup( skinTone?.MaterialGroup );

		// We only want the hair/hats so we won't use the logic built into Clothing
		var items = clothes.Clothing.Where( item =>
			item.Category == Clothing.ClothingCategory.Hair ||
			item.Category == Clothing.ClothingCategory.Hat
		);

		if ( !items.Any() )
			return;

		foreach ( var item in items )
		{
			var ent = new AnimatedEntity( item.Model, this );

			// Add a tag to the hat so we can reference it later.
			if ( item.Category == Clothing.ClothingCategory.Hat
				|| item.Category == Clothing.ClothingCategory.Hair )
				ent.Tags.Add( "head" );

			if ( !string.IsNullOrEmpty( item.MaterialGroup ) )
				ent.SetMaterialGroup( item.MaterialGroup );
		}

	}

	public void SetHatVisible( bool visible )
	{
		var hats = Children.OfType<AnimatedEntity>().Where( child => child.Tags.Has( "head" ) );

		foreach ( var hat in hats )
		{
			hat.EnableDrawing = visible;
		}
	}
}
