#region Assembly Sandbox.Game, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Game.dll
#endregion
using Sandbox;
using System.Runtime.InteropServices;

namespace Sandbox
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
