using Sandbox;

namespace Grubs.Terrain.SDFs
{
	public partial class Circle : SDF
	{
		[Net] public Vector2 _position { get; private set; }
		[Net] public float _radius { get; private set; }

		public Circle()
		{

		}

		public Circle( Vector2 position, float radius, ModifyType modifyType)
		{
			_position = position;
			_radius = radius;
			this.ModifyType = modifyType;
		}

		public override float GetDistance( Vector2 position )
		{
			return Vector2.DistanceBetween( _position, position ) - _radius;
		}

		public override string ToString()
		{
			return $"Circle, Center: {_position}, Radius: {_radius}, {ModifyType}";
		}
	}
}
