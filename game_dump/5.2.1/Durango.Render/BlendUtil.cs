using UnityEngine;

namespace Durango.Render;

public static class BlendUtil
{
	private static int _blendModeId = -1;

	private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");

	private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

	private static readonly int SrcAlphaBlend = Shader.PropertyToID("_SrcAlphaBlend");

	private static readonly int DstAlphaBlend = Shader.PropertyToID("_DstAlphaBlend");

	private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

	private static int BlendModeId
	{
		get
		{
			if (_blendModeId == -1)
			{
				_blendModeId = Shader.PropertyToID("_BlendMode");
			}
			return _blendModeId;
		}
	}

	public static BlendMode GetBlendMode(Material mat)
	{
		int blendModeId = BlendModeId;
		if (mat.HasProperty(blendModeId))
		{
			return (BlendMode)mat.GetInt(blendModeId);
		}
		return BlendMode.Invalid;
	}

	public static void SetBlendMode(Material mat, BlendMode mode)
	{
		mat.SetInt(BlendModeId, (int)mode);
		switch (mode)
		{
		case BlendMode.Opaque:
			mat.SetFloat(SrcBlend, 1f);
			mat.SetFloat(DstBlend, 0f);
			mat.SetFloat(SrcAlphaBlend, 1f);
			mat.SetFloat(DstAlphaBlend, 0f);
			break;
		case BlendMode.Transparent:
			mat.SetFloat(SrcBlend, 5f);
			mat.SetFloat(DstBlend, 10f);
			mat.SetFloat(SrcAlphaBlend, 1f);
			mat.SetFloat(DstAlphaBlend, 1f);
			break;
		case BlendMode.Additive:
			mat.SetFloat(SrcBlend, 5f);
			mat.SetFloat(DstBlend, 1f);
			mat.SetFloat(SrcAlphaBlend, 0f);
			mat.SetFloat(DstAlphaBlend, 10f);
			break;
		}
		int renderQueue = ((!(mat.shader != null)) ? 3000 : mat.shader.renderQueue);
		if (mode == BlendMode.Opaque)
		{
			if (mat.renderQueue >= 3000)
			{
				mat.renderQueue = -1;
			}
		}
		else if (mat.renderQueue < 3000)
		{
			mat.renderQueue = renderQueue;
		}
		mat.SetFloat(ZWrite, (mode == BlendMode.Opaque) ? 1 : 0);
		if (mode == BlendMode.Opaque)
		{
			mat.DisableKeyword("ALLOW_ALPHA_ON");
		}
		else
		{
			mat.EnableKeyword("ALLOW_ALPHA_ON");
		}
	}
}
