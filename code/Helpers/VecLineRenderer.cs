namespace Grubs.Helpers;

using System.Collections.Generic;
using System.Linq;
using global::Sandbox.Internal;

[Title( "Vector Line Renderer" )]
[Category( "Rendering" )]
[Icon( "show_chart" )]
public sealed class VecLineRenderer : Component, Component.ExecuteInEditor
{
	private SceneLineObject _so;

	[Group( "Points" ), Property, Sync]
	public List<Vector3> Points { get; set; } = new List<Vector3>();

	[Group( "Appearance" ), Property]
	public Texture LineTexture { get; set; }

	[Group( "Appearance" ), Property]
	public Gradient Color { get; set; } = global::Color.Cyan;


	[Group( "Appearance" ), Property]
	public Curve Width { get; set; }


	[Group( "Spline" ), Property, Range( 1f, 32f )]
	public int SplineInterpolation { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f )]
	public float SplineTension { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f )]
	public float SplineContinuity { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f )]
	public float SplineBias { get; set; }

	[Group( "End Caps" ), Property]
	public SceneLineObject.CapStyle StartCap { get; set; }

	[Group( "End Caps" ), Property]
	public SceneLineObject.CapStyle EndCap { get; set; }

	[Group( "Rendering" ), Property]
	public bool Wireframe { get; set; }

	[Group( "Rendering" ), Property]
	public bool Opaque { get; set; } = true;


	protected override void OnEnabled()
	{
		_so = new SceneLineObject( base.Scene.SceneWorld );
		_so.Transform = base.Transform.World;
	}

	protected override void OnDisabled()
	{
		_so?.Delete();
		_so = null;
	}

	protected override void OnPreRender()
	{
		if ( _so == null )
		{
			return;
		}

		if ( Points.Count == 0 )
		{
			_so.RenderingEnabled = false;
			return;
		}


		int num = Points.Count();
		if ( num <= 1 )
		{
			_so.RenderingEnabled = false;
			return;
		}

		_so.StartCap = StartCap;
		_so.EndCap = EndCap;
		_so.Wireframe = Wireframe;
		_so.Opaque = Opaque;
		_so.RenderingEnabled = true;
		_so.Transform = base.Transform.World;
		_so.Flags.CastShadows = true;

		RenderAttributes attributes = _so.Attributes;
		string k = "BaseTexture";
		Texture value = LineTexture;
		int mip = -1;
		attributes.Set( in k, in value, in mip );

		RenderAttributes attributes2 = _so.Attributes;
		k = "D_BLEND";
		mip = ((!Opaque) ? 1 : 0);
		attributes2.SetCombo( in k, in mip );
		_so.StartLine();

		if ( num == 2 || SplineInterpolation == 1 )
		{
			int num2 = 0;
			foreach ( Vector3 item in Points )
			{
				Vector3 pos = item;
				float time = (float)num2 / (float)num;
				_so.AddLinePoint( in pos, Color.Evaluate( time ), Width.Evaluate( time ) );
				num2++;
			}
		}
		else
		{
			int num3 = 0;
			int num4 = SplineInterpolation.Clamp( 1, 100 );
			int num5 = (num - 1) * num4;
			foreach ( Vector3 item2 in Points.TcbSpline( num4, SplineTension, SplineContinuity, SplineBias ) )
			{
				Vector3 pos2 = item2;
				float time2 = (float)num3 / (float)num5;
				_so.AddLinePoint( in pos2, Color.Evaluate( time2 ), Width.Evaluate( time2 ) );
				num3++;
			}
		}

		_so.EndLine();
	}
}
