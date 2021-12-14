﻿using Sandbox;
using System;

namespace TerryForm.Weapons
{
	public partial class PowerArrow : RenderEntity
	{
		public Material Material = Material.Load( "materials/minigolf.arrow.vmat" );

		public Vector3 Direction = Vector3.Zero;
		public float Power = 0.0f;

		protected void DrawArrow( SceneObject obj, Vector3 startPos, Vector3 endPos, Vector3 direction, Vector3 size, Color color )
		{
			// vbos are drawn relative to world position
			startPos -= Position;
			endPos -= Position;

			var vertexBuffer = Render.GetDynamicVB( true );

			// Line
			Vertex a = new( startPos - size, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			Vertex b = new( startPos + size, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			Vertex c = new( endPos + size * 3f, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			Vertex d = new( endPos - size * 3f, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vertexBuffer.Add( a );
			vertexBuffer.Add( b );
			vertexBuffer.Add( c );
			vertexBuffer.Add( d );

			vertexBuffer.AddTriangleIndex( 4, 3, 2 );
			vertexBuffer.AddTriangleIndex( 2, 1, 4 );

			// Add the arrow tip
			Vertex e = new( endPos + size * 4.75f, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			Vertex f = new( endPos - size * 4.75f, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
			Vertex g = new( endPos + direction * 14, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );

			vertexBuffer.Add( e );
			vertexBuffer.Add( f );
			vertexBuffer.Add( g );
			vertexBuffer.AddTriangleIndex( 1, 2, 3 );

			Render.Set( "color", color );

			vertexBuffer.Draw( Material );
		}

		public override void DoRender( SceneObject obj )
		{
			if ( Power.AlmostEqual( 0.0f ) )
				return;

			Render.SetLighting( obj );

			var startPos = Position;
			var endPos = Position + (Direction * Power);
			var size = Vector3.Cross( Direction, Vector3.Right ) * 2f;

			var color = new ColorHsv( Math.Clamp( 60 - Power + 50, 0, 60 ), 0.8f, 0.6f, 1f );
			DrawArrow( obj, startPos, endPos, Direction, size, color );
		}
	}
}
