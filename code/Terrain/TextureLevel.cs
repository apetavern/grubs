namespace Grubs;

[GameResource( "TextureLevel", "tlvl", "Texture level data", Icon = "cottage" )]
public partial class TextureLevel : GameResource
{
	[Property] 
	public Texture texture { get; set; }
}
