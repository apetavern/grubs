using Sandbox;
using System;

namespace Grubs.Terrain.SDFs
{
	public partial class Rectangle : SDF
	{

		[Net] public Vector2 _position { get; private set; }
		[Net] public Vector2 _size { get; private set; }

		public Rectangle()
		{

		}

		public Rectangle( Vector2 position, Vector2 size, ModifyType modifyType )
		{
			_position = position;
			_size = size;
			this.ModifyType = modifyType;
		}

		public override float GetDistance( Vector2 position )
		{
			Vector2 shiftedPosition = position - _position;

			float qX = MathF.Abs( shiftedPosition.x ) - _size.x;
			float qY = MathF.Abs( shiftedPosition.y ) - _size.y;

			float componentX = MathF.Max( qX, 0 );
			float componentY = MathF.Max( qY, 0 );

			return new Vector2( componentX, componentY ).Length + MathF.Min( MathF.Max( qX, qY ), 0 );
		}
	}
}
