using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandbox.Sdf;

internal record struct Sdf2DArrayData( byte[] Samples, int BaseIndex, int Size, int RowStride )
{
	public byte this[int x, int y]
	{
		get
		{
			if ( x < -1 || x > Size + 1 || y < -1 || y > Size + 1 )
			{
				return 191;
			}

			return Samples[BaseIndex + x + y * RowStride];
		}
	}
}

/// <summary>
/// Array containing raw SDF samples for a <see cref="Sdf2DChunk"/>.
/// </summary>
public partial class Sdf2DArray : SdfArray<ISdf2D>
{
	/// <summary>
	/// Array containing raw SDF samples for a <see cref="Sdf2DChunk"/>.
	/// </summary>
	public Sdf2DArray()
		: base( 2 )
	{
	}

	/// <inheritdoc />
	protected override Texture CreateTexture()
	{
		return new Texture2DBuilder()
			.WithFormat( ImageFormat.I8 )
			.WithSize( ArraySize, ArraySize )
			.WithData( FrontBuffer )
			.WithAnonymous( true )
			.Finish();
	}

	/// <inheritdoc />
	protected override void UpdateTexture( Texture texture )
	{
		texture.Update( FrontBuffer );
	}

	private ((int X, int Y) Min, (int X, int Y) Max, Transform transform) GetSampleRange( Rect bounds )
	{
		var (minX, maxX, minLocalX, _) = GetSampleRange( bounds.Left, bounds.Right );
		var (minY, maxY, minLocalY, _) = GetSampleRange( bounds.Top, bounds.Bottom );

		var min = new Vector3( minLocalX, minLocalY, 0f );
		
		return ((minX, minY), (maxX, maxY), new Transform( min, Rotation.Identity, UnitSize ) );
	}

	/// <summary>
	/// Implements the logic for adding pre-sampled SDF data to the back buffer.
	/// </summary>
	/// <param name="samples">The array of pre-sampled distance values.</param>
	/// <param name="min">The minimum grid coordinate of the sampled area.</param>
	/// <param name="size">The dimensions of the sampled area.</param>
	/// <returns>True if any value in the buffer was changed.</returns>
	private bool AddImpl( float[] samples, (int X, int Y) min, (int X, int Y) size )
	{
		var max = (X: min.X + size.X, Y: min.Y + size.Y);
		var maxDist = Quality.MaxDistance;
		var changed = false;

		for ( var y = min.Y; y < max.Y; ++y )
		{
			var srcIndex = (y - min.Y) * size.X;
			var dstIndex = min.X + y * ArraySize;

			for ( var x = min.X; x < max.X; ++x, ++srcIndex, ++dstIndex )
			{
				var sampled = samples[srcIndex];

				if ( sampled >= maxDist ) continue;

				var encoded = Encode( sampled );

				var oldValue = BackBuffer[dstIndex];
				var newValue = Math.Min( encoded, oldValue );
				BackBuffer[dstIndex] = newValue;

				changed |= oldValue != newValue;
			}
		}

		return changed;
	}
	
	/// <summary>
	/// Implements the logic for subtracting pre-sampled SDF data from the back buffer.
	/// </summary>
	/// <param name="samples">The array of pre-sampled distance values.</param>
	/// <param name="min">The minimum grid coordinate of the sampled area.</param>
	/// <param name="size">The dimensions of the sampled area.</param>
	/// <returns>True if any value in the buffer was changed.</returns>
	private bool SubtractImpl( float[] samples, (int X, int Y) min, (int X, int Y) size )
	{
		var max = (X: min.X + size.X, Y: min.Y + size.Y);
		var maxDist = Quality.MaxDistance;
		var changed = false;

		for ( var y = min.Y; y < max.Y; ++y )
		{
			var srcIndex = (y - min.Y) * size.X;
			var dstIndex = min.X + y * ArraySize;

			for ( var x = min.X; x < max.X; ++x, ++srcIndex, ++dstIndex )
			{
				var sampled = samples[srcIndex];

				if ( sampled >= maxDist ) continue;

				var encoded = Encode( sampled );
				
				var oldValue = BackBuffer[dstIndex];
				var newValue = Math.Max( (byte)(byte.MaxValue - encoded), oldValue );

				BackBuffer[dstIndex] = newValue;

				changed |= oldValue != newValue;
			}
		}

		return changed;
	}

	/// <inheritdoc />
	public override async Task<bool> AddAsync<T>( T sdf )
	{
		var (min, max, transform) = GetSampleRange( sdf.Bounds );
		var maxDist = Quality.MaxDistance;
		var size = (X: max.X - min.X, Y: max.Y - min.Y);
		
		var samples = ArrayPool<float>.Shared.Rent( size.X * size.Y );
		
		var changed = false;

		try
		{
			await sdf.SampleRangeAsync( transform, samples, size );
       
			await GameTask.WorkerThread();

			changed |= AddImpl( samples, min, size );
		}
		finally
		{
			ArrayPool<float>.Shared.Return( samples );
		}

		if ( changed )
		{
			SwapBuffers();
			MarkChanged();
		}

		return changed;
	}

	/// <inheritdoc />
	public override async Task<bool> SubtractAsync<T>( T sdf )
	{
		var (min, max, transform) = GetSampleRange( sdf.Bounds );
		var size = (X: max.X - min.X, Y: max.Y - min.Y);

		var samples = ArrayPool<float>.Shared.Rent( size.X * size.Y );

		var changed = false;

		try
		{
			await sdf.SampleRangeAsync( transform, samples, size );
       
			await GameTask.WorkerThread();

			changed |= SubtractImpl( samples, min, size );
		}
		finally
		{
			ArrayPool<float>.Shared.Return( samples );
		}

		if ( changed )
		{
			SwapBuffers();
			MarkChanged();
		}

		return changed;
	}

	public override async Task<bool> RebuildAsync( IEnumerable<ChunkModification<ISdf2D>> modifications )
	{
		Array.Fill( BackBuffer, (byte)255 );

		var samples = ArrayPool<float>.Shared.Rent( ArraySize * ArraySize * ArraySize );

		try
		{
			foreach ( var modification in modifications )
			{
				var (min, max, transform) = GetSampleRange( modification.Sdf.Bounds );
				var size = (X: max.X - min.X, Y: max.Y - min.Y);
				
				await modification.Sdf.SampleRangeAsync( transform, samples, size );
				
				// Having a WorkerThread here occasionally crashes s&box in some context.
				// Seems to run fine without it.
				// await GameTask.WorkerThread();

				switch ( modification.Operator )
				{
					case Operator.Add:
						AddImpl( samples, min, size );
						break;
					case Operator.Subtract:
						SubtractImpl( samples, min, size );
						break;
				}
			}
		}
		finally
		{
			ArrayPool<float>.Shared.Return( samples );
		}

		SwapBuffers();
		MarkChanged();

		return true;
	}

	internal void WriteTo( Sdf2DMeshWriter writer, Sdf2DLayer layer, bool renderMesh, bool collisionMesh )
	{
		if ( writer.Samples == null || writer.Samples.Length < FrontBuffer.Length )
		{
			writer.Samples = new byte[FrontBuffer.Length];
		}

		Array.Copy( FrontBuffer, writer.Samples, FrontBuffer.Length );

		var resolution = layer.Quality.ChunkResolution;

		var data = new Sdf2DArrayData( writer.Samples, Margin * ArraySize + Margin, resolution, ArraySize );

		writer.Write( data, layer, renderMesh, collisionMesh );
	}
}
