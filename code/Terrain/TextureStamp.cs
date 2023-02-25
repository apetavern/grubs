using Sandbox.Csg;

namespace Grubs;

[GameResource( "TextureStamp", "tsta", "Texture for stamping", Icon = "build" )]
public partial class TextureStamp : GameResource
{
	[Property]
	public Texture texture { get; set; }

	public CsgMaterial material { get; set; }
}
