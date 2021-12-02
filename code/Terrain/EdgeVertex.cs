using System.Runtime.InteropServices;
using Sandbox;

namespace TerryForm.Terrain
{
	[StructLayout( LayoutKind.Sequential )]
	public struct EdgeVertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Color32 color;

		public static readonly VertexAttribute[] Layout =
		{
			new VertexAttribute( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.Normal, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.Color, VertexAttributeFormat.UInt8, 4 )
		};
		public EdgeVertex( Vector3 position, Vector3 normal, Color32 color )
		{
			this.position = position;
			this.normal = normal;
			this.color = color;
		}
	}
}
