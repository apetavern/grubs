using Sandbox;
using System.Runtime.InteropServices;

namespace Grubs.Terrain
{
	[StructLayout( LayoutKind.Sequential )]
	public struct TerrainVertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector2 texcoord;

		public static readonly VertexAttribute[] Layout =
		{
			new VertexAttribute( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.Normal, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.Tangent, VertexAttributeFormat.Float32, 3 ),
			new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2 )
		};
		public TerrainVertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textcoord )
		{
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
			this.texcoord = textcoord;
		}
	}
}
