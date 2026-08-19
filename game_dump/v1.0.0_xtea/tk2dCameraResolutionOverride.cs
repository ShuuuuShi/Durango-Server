using System;
using UnityEngine;

[Serializable]
public class tk2dCameraResolutionOverride
{
	public enum MatchByType
	{
		Resolution,
		AspectRatio,
		Wildcard
	}

	public enum AutoScaleMode
	{
		None,
		FitWidth,
		FitHeight,
		FitVisible,
		StretchToFit,
		ClosestMultipleOfTwo,
		PixelPerfect,
		Fill
	}

	public enum FitMode
	{
		Constant,
		Center
	}

	public string name;

	public MatchByType matchBy;

	public int width;

	public int height;

	public float aspectRatioNumerator = 4f;

	public float aspectRatioDenominator = 3f;

	public float scale = 1f;

	public Vector2 offsetPixels = new Vector2(0f, 0f);

	public AutoScaleMode autoScaleMode;

	public FitMode fitMode;

	public static tk2dCameraResolutionOverride DefaultOverride
	{
		get
		{
			tk2dCameraResolutionOverride tk2dCameraResolutionOverride2 = new tk2dCameraResolutionOverride();
			tk2dCameraResolutionOverride2.name = "Override";
			tk2dCameraResolutionOverride2.matchBy = MatchByType.Wildcard;
			tk2dCameraResolutionOverride2.autoScaleMode = AutoScaleMode.FitVisible;
			tk2dCameraResolutionOverride2.fitMode = FitMode.Center;
			return tk2dCameraResolutionOverride2;
		}
	}

	public bool Match(int pixelWidth, int pixelHeight)
	{
		switch (matchBy)
		{
		case MatchByType.Wildcard:
			return true;
		case MatchByType.Resolution:
			return pixelWidth == width && pixelHeight == height;
		case MatchByType.AspectRatio:
		{
			float num = (float)pixelWidth * aspectRatioDenominator / aspectRatioNumerator;
			return Mathf.Approximately(num, (float)pixelHeight);
		}
		default:
			return false;
		}
	}

	public void Upgrade(int version)
	{
		if (version == 0)
		{
			matchBy = (((width == -1 && height == -1) || (width == 0 && height == 0)) ? MatchByType.Wildcard : MatchByType.Resolution);
		}
	}
}
