using System.Collections.Generic;
using UnityEngine;

public class RendererProxy
{
	public struct RendererSet
	{
		public readonly Renderer Renderer;

		public readonly Shader Shader;

		private Shader _transparentShader;

		public Shader TransparentShader
		{
			get
			{
				if ((Object)(object)_transparentShader == (Object)null)
				{
					_transparentShader = ShaderLibrary.GetTransparent(Shader);
				}
				return _transparentShader;
			}
		}

		public RendererSet(Renderer renderer)
		{
			Renderer = renderer;
			Shader = ((!((Object)(object)renderer == (Object)null)) ? renderer.sharedMaterial.shader : null);
			_transparentShader = null;
		}
	}

	private Color _color = Color.white;

	private readonly List<RendererSet> _rendererSets = new List<RendererSet>();

	public Color Color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!(_color == value))
			{
				_color = value;
				ApplyColors();
			}
		}
	}

	public void Clear()
	{
		_rendererSets.Clear();
	}

	public void UpdateRenderers(IList<Renderer> renderers, GameObject ignore = null)
	{
		Clear();
		if (renderers == null)
		{
			return;
		}
		int i = 0;
		for (int count = renderers.Count; i < count; i++)
		{
			Renderer renderer = renderers[i];
			if (IsColorable(renderer, ignore))
			{
				_rendererSets.Add(new RendererSet(renderer));
			}
		}
		ApplyColors();
	}

	private static bool IsColorable(Renderer renderer, GameObject ignoreObj)
	{
		if ((Object)(object)renderer == (Object)null || (Object)(object)renderer.sharedMaterial == (Object)null || !renderer.sharedMaterial.HasProperty("_Color"))
		{
			return false;
		}
		string name = ((Object)renderer).name;
		if (name.StartsWith("site") || name.StartsWith("LightMask") || name.StartsWith("drawing_board"))
		{
			return false;
		}
		if ((Object)(object)ignoreObj != (Object)null && NGUITools.IsChild(ignoreObj.transform, ((Component)renderer).transform))
		{
			return false;
		}
		return (Object)(object)renderer.sharedMaterial.shader != (Object)null;
	}

	private void ApplyColors()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _rendererSets.Count; i < count; i++)
		{
			Renderer renderer = _rendererSets[i].Renderer;
			renderer.material.shader = ((!(Color.a < 1f)) ? _rendererSets[i].Shader : _rendererSets[i].TransparentShader);
			renderer.material.color = Color;
		}
	}
}
