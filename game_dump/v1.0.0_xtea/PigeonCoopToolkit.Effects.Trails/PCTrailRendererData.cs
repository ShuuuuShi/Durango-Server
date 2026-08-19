using System;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

[Serializable]
public class PCTrailRendererData
{
	public Material TrailMaterial;

	public float Lifetime = 1f;

	public AnimationCurve SizeOverLife = new AnimationCurve();

	public Gradient ColorOverLife;

	public bool StretchSizeToFit;

	public bool StretchColorToFit;

	public float MaterialTileLength;

	public bool UseForwardOverride;

	public Vector3 ForwardOverride;

	public bool ForwardOverrideRelative;
}
