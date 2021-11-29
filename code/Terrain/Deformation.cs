using System.Collections;
using System.Collections.Generic;
using Sandbox;
using System;

public struct Deformation
{
	public Vector3 Position { get; private set; }
	public bool Boolean { get; private set; }
	public float Radius { get; private set; }

	public Deformation( Vector3 position, bool boolean, float radius )
	{
		Position = position;
		Boolean = boolean;
		Radius = radius;
	}

	public override string ToString()
	{
		return $"Deformation [{Position}, {Boolean}, {Radius}]";
	}
}
