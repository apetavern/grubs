using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs;
[GameResource( "TextureLevel", "tlvl", "Texture level data", Icon = "cottage" )]
public partial class TextureLevel : GameResource
{
	[Property] public Texture texture { get; set; }
}
