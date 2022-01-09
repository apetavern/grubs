using Sandbox;
using System;
using System.Collections.Generic;

namespace Grubs.Terrain
{
	//Solid and empty self explanatory. Complex = result of a march that wasnt solid or empty. Children = node has children.
	public enum NodeStatus { Solid, Empty, Complex, Children}

	public class Quadtree
	{
		public Entity Entity;
		//MSB = Y, LSB = X
		public float SDF00;
		public float SDF01;
		public float SDF10;
		public float SDF11;
		public NodeStatus ContainmentStatus;
		public Quadtree Child00;
		public Quadtree Child01;
		public Quadtree Child10;
		public Quadtree Child11;
		public Quadtree Parent;
		public (List<Vector3> vertices, List<int> indices) meshData;

		public Quadtree()
		{
			ContainmentStatus = NodeStatus.Empty;
			SDF00 = 1;
			SDF01 = 1;
			SDF10 = 1;
			SDF11 = 1;
			meshData = (new List<Vector3>(), new List<int>());
		}

		public void ForeachChild( Action<Quadtree> action)
		{
			if(this.ContainmentStatus == NodeStatus.Children)
			{
				action( Child00 );
				action( Child01 );
				action( Child10 );
				action( Child11 );
			}
		}

		public void deleteEntities()
		{
			if(this.Entity != null)
			{
				this.Entity.DeleteAsync( 0.0f );
				this.Entity = null;
			}
			ForeachChild( ( Quadtree qt ) => qt.deleteEntities() );
		}

		public void ClearChildren()
		{
			Child00 = null;
			Child01 = null;
			Child10 = null;
			Child11 = null;
		}

		public void CreateChildren()
		{
			Child00 = new Quadtree();
			Child01 = new Quadtree();
			Child10 = new Quadtree();
			Child11 = new Quadtree();

			Child00.Parent = this;
			Child01.Parent = this;
			Child10.Parent = this;
			Child11.Parent = this;

			Child00.ContainmentStatus = this.ContainmentStatus;
			Child01.ContainmentStatus = this.ContainmentStatus;
			Child10.ContainmentStatus = this.ContainmentStatus;
			Child11.ContainmentStatus = this.ContainmentStatus;



			/*
			float leftMidpoint = (SDF00 + SDF10) / 2;
			float rightMidpoint = (SDF01 + SDF11) / 2;
			float topMidpoint = (SDF10 + SDF11) / 2;
			float bottomMidpoint = (SDF00 + SDF01) / 2;
			float midpoint = (topMidpoint + bottomMidpoint) / 2;
			//Working clockwise from the bottom-left node's bottom-left measurement around the edges
			Child00.SDF00 = SDF00;
			Child00.SDF10 = leftMidpoint;
			Child10.SDF00 = leftMidpoint;
			Child10.SDF10 = SDF10;
			Child10.SDF11 = topMidpoint;
			Child11.SDF10 = topMidpoint;
			Child11.SDF11 = SDF11;
			Child11.SDF01 = rightMidpoint;
			Child01.SDF01 = rightMidpoint;
			Child01.SDF01 = SDF01;
			Child01.SDF00 = bottomMidpoint;
			Child00.SDF01 = bottomMidpoint;

			Child00.SDF11 = Child01.SDF10 = Child10.SDF01 = Child11.SDF00 = midpoint;
			*/
		}
	}
}
