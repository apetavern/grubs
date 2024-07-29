using Sandbox.Sdf;
using System;
using System.Collections.Generic;

namespace Sandbox.Polygons;

internal static class Helpers
{
	public static Vector2 NormalizeSafe( in Vector2 vec )
	{
		var length = vec.Length;

		if ( length > 9.9999997473787516E-06 )
		{
			return vec / length;
		}
		else
		{
			return 0f;
		}
	}

	public static Vector2 Rotate90( Vector2 v )
	{
		return new Vector2( v.y, -v.x );
	}

	public static float Cross( Vector2 a, Vector2 b )
	{
		return a.x * b.y - a.y * b.x;
	}

	public static bool LineSegmentsIntersect( Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1 )
	{
		return Math.Sign( Cross( a0 - b0, b1 - b0 ) ) != Math.Sign( Cross( a1 - b0, b1 - b0 ) )
			&& Math.Sign( Cross( b0 - a0, a1 - a0 ) ) != Math.Sign( Cross( b1 - a0, a1 - a0 ) );
	}

	public static Vector3 RotateNormal( Vector3 oldNormal, float sin, float cos )
	{
		var normal2d = new Vector2( oldNormal.x, oldNormal.y );

		if ( normal2d.LengthSquared <= 0.000001f )
		{
			return oldNormal;
		}

		normal2d = NormalizeSafe( normal2d );

		return new Vector3( normal2d.x * cos, normal2d.y * cos, sin ).Normal;
	}

	public static float GetEpsilon( Vector2 vec, float frac = 0.0001f )
	{
		return Math.Max( Math.Abs( vec.x ), Math.Abs( vec.y ) ) * frac;
	}

	public static float GetEpsilon( Vector2 a, Vector2 b, float frac = 0.0001f )
	{
		return Math.Max( GetEpsilon( a, frac ), GetEpsilon( b, frac ) );
	}

	public static void UpdateMesh<T>( this Mesh mesh, VertexAttribute[] layout, List<T> vertices, List<int> indices )
		where T : unmanaged
	{
		if ( !mesh.HasIndexBuffer )
		{
			mesh.CreateVertexBuffer( vertices.Count, layout, vertices );
			mesh.CreateIndexBuffer( indices.Count, indices );
		}
		else if ( indices.Count > 0 && vertices.Count > 0 )
		{
			mesh.SetIndexBufferSize( indices.Count );
			mesh.SetVertexBufferSize( vertices.Count );

			mesh.SetVertexBufferData( vertices );
			mesh.SetIndexBufferData( indices );
		}

		mesh.SetIndexRange( 0, indices.Count );
	}
}

internal record DebugDump(
	string Exception,
	string EdgeLoops,
	EdgeStyle EdgeStyle,
	float EdgeWidth,
	int EdgeFaces );
