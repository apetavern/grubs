using System;
using System.Collections.Generic;
using Sandbox.Diagnostics;

namespace Sandbox.Sdf;

/// <summary>
/// Main entity for creating a set of 2D surfaces that can be added to and subtracted from.
/// Each surface is aligned to the same plane, but can have different offsets, depths, and materials.
/// </summary>
[Title( "SDF 2D World" )]
public partial class Sdf2DWorld : SdfWorld<Sdf2DWorld, Sdf2DChunk, Sdf2DLayer, (int X, int Y), Sdf2DArray, ISdf2D>
{
	[Property] public Curve Curve { get; set; }
	public static Curve TerrainCurve { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		TerrainCurve = Curve;
	}

	/// <inheritdoc />
	public override int Dimensions => 2;

	private (int MinX, int MinY, int MaxX, int MaxY) GetChunkRange( Rect bounds, WorldQuality quality )
	{
		var unitSize = quality.UnitSize;

		var min = (bounds.TopLeft - quality.MaxDistance - unitSize) / quality.ChunkSize;
		var max = (bounds.BottomRight + quality.MaxDistance + unitSize) / quality.ChunkSize;

		var minX = (int)MathF.Floor( min.x );
		var minY = (int)MathF.Floor( min.y );

		var maxX = (int)MathF.Ceiling( max.x );
		var maxY = (int)MathF.Ceiling( max.y );

		return (minX, minY, maxX, maxY);
	}

	/// <inheritdoc />
	protected override IEnumerable<(int X, int Y)> GetAffectedChunks<T>( T sdf, WorldQuality quality )
	{
		var (minX, minY, maxX, maxY) = GetChunkRange( sdf.Bounds, quality );

		for ( var y = minY; y < maxY; ++y )
			for ( var x = minX; x < maxX; ++x )
			{
				yield return (x, y);
			}
	}

	protected override bool AffectsChunk<T>( T sdf, WorldQuality quality, (int X, int Y) chunkKey )
	{
		var (minX, minY, maxX, maxY) = GetChunkRange( sdf.Bounds, quality );

		return chunkKey.X >= minX && chunkKey.X < maxX
			&& chunkKey.Y >= minY && chunkKey.Y < maxY;
	}

	public void ClearAndReadData( ref ByteStream msg )
	{
		ClearCount = 0;
		
		var clearCount = msg.Read<int>();
		var msgCount = msg.Read<int>();

		using var clientMods = AllowClientModifications();

		if ( clearCount < ClearCount )
		{
			return;
		}

		if ( clearCount > ClearCount )
		{
			ClearCount = clearCount;
			_ = ClearAsync();

			Assert.AreEqual( 0, Modifications.Count );
		}

		ISdf<ISdf2D>.EnsureTypesRegistered();

		NetRead_TypeReaders.Clear();

		var index = 0;
		foreach ( var (_, reader) in ISdf<ISdf2D>.RegisteredTypes )
		{
			NetRead_TypeReaders[index++] = reader;
		}

		ReadModifications( ref msg, msgCount, NetRead_TypeReaders );
		// ReadRange( ref msg, msgCount, NetRead_TypeReaders );
	}

	private void ReadModifications( ref ByteStream reader, int count,
		IReadOnlyDictionary<int, SdfReader<ISdf2D>> sdfTypes )
	{
		List<Modification<Sdf2DLayer, ISdf2D>> mods = [];
		
		for ( var i = 0; i < count; ++i )
		{
			var op = (Operator)reader.Read<byte>();
			var resId = reader.Read<int>();
			var res = ResourceLibrary.Get<Sdf2DLayer>( resId );

			var sdf = ISdf<ISdf2D>.Read( ref reader, sdfTypes );

			var modification = new Modification<Sdf2DLayer, ISdf2D>( sdf, res, op );
			mods.Add( modification );
		}

		_ = SetModificationsAsync( mods );
	}
}
