using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Csg
{
    partial class CsgSolid
    {
        public string Export()
        {
            using ( var writer = new StringWriter() )
            {
                Export( writer );

                return writer.ToString();
            }
        }

        public void Export( TextWriter writer )
        {
            writer.WriteLine(
                "<!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:generic:version{7412167c-06e9-4698-aff2-e63eb59037e7} -->" );
            writer.WriteLine( "{" );
            writer.WriteLine( "\tdata =" );
            writer.WriteLine( "\t{" );
            writer.WriteLine( "\t\tConvexSolids =" );
            writer.WriteLine( "\t\t[" );

            foreach ( var (_, cell) in _grid )
            {
                foreach ( var hull in cell.Hulls )
                {
                    writer.WriteLine("\t\t\t{");
                    writer.WriteLine( "\t\t\t\tPlanes =" );
                    writer.WriteLine( "\t\t\t\t[" );

                    foreach ( var face in hull.Faces )
                    {
                        var n = face.Plane.Normal;

                        writer.WriteLine("\t\t\t\t\t{");
                        writer.WriteLine( $"\t\t\t\t\t\tNormal = \"{n.x:r},{n.y:r},{n.z:r}\"" );
                        writer.WriteLine( $"\t\t\t\t\t\tDistance = {face.Plane.Distance:r}" );
                        writer.WriteLine( "\t\t\t\t\t}," );
                    }

                    writer.WriteLine( "\t\t\t\t]" );
                    writer.WriteLine( "\t\t\t}," );
                }
            }

            writer.WriteLine( "\t\t]" );
            writer.WriteLine( "\t}" );
            writer.WriteLine( "}" );
        }
    }
}