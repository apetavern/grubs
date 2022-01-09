using System.Collections.Generic;
using Sandbox;

namespace Grubs.Terrain
{
	public partial class TerrainEntity : Entity
	{
		[Net] public IList<SDF> SDFs { get; set; } = new List<SDF>();
		private int listIndex = 0;


		[ClientRpc]
		public void ClientReset()
		{
			Reset();
		}

		public void Reset()
		{
			listIndex = 0;

			Quadtree.CreateGrid();
			Quadtree.BuildModels( true );

			if ( IsServer )
			{
				SDFs.Clear();
				ClientReset();
			}
		}

		public TerrainEntity()
		{
			Transmit = TransmitType.Always;
			Name = "TerrainEntity";
		}

		[Event.Tick]
		public void Simulate()
		{
			int count = SDFs.Count;

			int toProcess = count - listIndex;

			// return if there is nothing to process
			if ( toProcess == 0 )
				return;

			//Log.Info( $"Processing {toProcess} new SDFs..." );
			Stopwatch watch = new Stopwatch();
			for ( int i = 0; i < toProcess; i++ )
			{
				SDF sdf = SDFs[listIndex];
				Quadtree.Update( sdf );
				//Log.Info( $"{sdf} took {watch.Lap()}ms!" );
				listIndex++;
			}
		}
	}

}
