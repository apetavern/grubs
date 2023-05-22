﻿using Sandbox;

namespace Grubs;

public partial class Player : Entity
{
	[Net]
	public IList<Grub> Grubs { get; private set; }

	[Net]
	public Grub ActiveGrub { get; private set; }

	[Net]
	public IList<Gadget> Gadgets { get; private set; }

	[Net]
	public string SteamName { get; private set; }

	[Net]
	public long SteamId { get; private set; }

	/// <summary>
	/// The clothing the player joined the game with.
	/// </summary>
	[Net]
	public string AvatarClothingData { get; private set; }

	public bool IsDead => Grubs.All( grub => grub.LifeState == LifeState.Dead );

	public bool IsAvailableForTurn => !IsDead && !IsDisconnected;

	[BindComponent]
	public Inventory Inventory { get; }

	public GrubsCamera GrubsCamera { get; } = new();

	public bool IsTurn
	{
		get
		{
			return GamemodeSystem.Instance.ActivePlayer == this && !GamemodeSystem.Instance.TurnIsChanging;
		}
	}

	public bool IsDisconnected => !Client.IsValid();

	public Player()
	{
		Transmit = TransmitType.Always;
	}

	public Player( IClient client ) : this()
	{
		SteamName = client.Name;
		SteamId = client.SteamId;
		SaveClientClothes( client );
	}

	public override void ClientSpawn()
	{
		if ( IsLocalPawn )
		{
			SelectedColor = Random.Shared.FromList( ColorPresets );
			SelectedCosmeticIndex = -1;
			PopulateGrubNames();
		}
	}

	public override void Spawn()
	{
		Tags.Add( Tag.IgnoreReset );

		Components.Create<Inventory>();
	}

	public override void Simulate( IClient client )
	{
		Inventory.Simulate( client );
		Grubs.Simulate( client );
		Gadgets.Simulate( client );

		if ( IsTurn )
			ActiveGrub?.UpdateInputFromOwner( MoveInput, LookInput );
	}

	public override void FrameSimulate( IClient client )
	{
		GrubsCamera.FrameSimulate( client );

		foreach ( var grub in Grubs )
		{
			grub?.FrameSimulate( client );
		}
	}

	public void Respawn()
	{
		Inventory.Clear();
		Inventory.GiveDefaultLoadout();

		Color = !Client.IsBot ? SelectedColor : Random.Shared.FromList( ColorPresets );
		Grubs.Clear();
		CreateGrubs();
	}

	private void CreateGrubs()
	{
		var grubNames = string.IsNullOrEmpty( GrubNames ) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>( GrubNames );
		for ( int i = 0; i < GrubsConfig.GrubCount; i++ )
		{
			Grubs.Add( new Grub( this ) { Owner = this, Name = ParseGrubName( grubNames.ElementAtOrDefault( i ) ) } );
		}

		ActiveGrub = Grubs.First();
	}

	public void PickNextGrub()
	{
		RotateGrubs();

		while ( ActiveGrub.LifeState is LifeState.Dead or LifeState.Dying )
		{
			RotateGrubs();
		}
	}

	private void RotateGrubs()
	{
		var current = Grubs[0];
		current.EyeRotation = Rotation.Identity;

		Grubs.RemoveAt( 0 );
		Grubs.Add( current );

		ActiveGrub = Grubs[0];
	}

	public void EndTurn()
	{
		if ( !ActiveGrub.IsValid() || !ActiveGrub.ActiveWeapon.IsValid() )
			return;

		if ( Inventory.ActiveWeapon.IsCharging() )
			Inventory.ActiveWeapon.Fire();

		Inventory.SetActiveWeapon( null, true );
	}

	public int GetTotalGrubHealth()
	{
		return (int)Grubs.Sum( g => g.Health );
	}

	private void SaveClientClothes( IClient client )
	{
		var clothes = new ClothingContainer();
		clothes.LoadFromClient( client );

		clothes.Clothing.RemoveAll(
			c => c.Category is not Clothing.ClothingCategory.Hair
			and not Clothing.ClothingCategory.Hat
			and not Clothing.ClothingCategory.Facial
			and not Clothing.ClothingCategory.Skin
		);

		AvatarClothingData = clothes.Serialize();
	}

	private string ParseGrubName( string name )
	{
		return string.IsNullOrWhiteSpace( name ) ? Random.Shared.FromList( GrubNamePresets ) : name.Trim();
	}
}
