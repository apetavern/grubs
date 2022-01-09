using Sandbox;

namespace Grubs.Terrain.SDFs
{
	public enum ModifyType
	{
		FILL, EMPTY
	}

	public abstract partial class SDF : BaseNetworkable
	{
		[Net] public ModifyType ModifyType { get; protected set; }
		public abstract float GetDistance( Vector2 position );
	}
}
