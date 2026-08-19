using System;
using UnityEngine;

namespace Durango.UI.Control;

public class BlurTexture : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UITexture _blurTexture;

	[SerializeField]
	private Material _blurMaterial;

	[SerializeField]
	private Color _tintColor;

	[SerializeField]
	[Range(0f, 10f)]
	private float _spacing;

	[SerializeField]
	[Range(0f, 2f)]
	private float _vibrancy;

	private UIBase.AnchorType _anchor = UIBase.AnchorType.Base;

	private static readonly int ShaderTintColor = Shader.PropertyToID("_TintColor");

	private static readonly int ShaderSize = Shader.PropertyToID("_Size");

	private static readonly int ShaderVibrancy = Shader.PropertyToID("_Vibrancy");

	public Color TintColor => _tintColor;

	public float Spacing => _spacing;

	public float Vibrancy => _vibrancy;

	public void Init()
	{
		_blurTexture.material = UnityEngine.Object.Instantiate(_blurMaterial);
		if (_blurTexture.GetComponent<Collider>() != null)
		{
			UIEventListener.Get(_blurTexture.gameObject).onClick = OnClose;
		}
		Show(show: false);
	}

	private void OnValidate()
	{
		if (Application.isPlaying && _blurTexture != null && _blurMaterial != null)
		{
			UpdateBlurTexture();
		}
	}

	public void Show(bool show, UIBase.AnchorType anchorType = UIBase.AnchorType.Base)
	{
		if (_blurTexture == null)
		{
			return;
		}
		if (show)
		{
			UpdateBlurTexture();
			if (_panel != null)
			{
				if (_anchor != anchorType || !_panel.isAnchored)
				{
					_anchor = anchorType;
					UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(_anchor);
					if (rootAnchor == null)
					{
						return;
					}
					int left = ((_anchor != UIBase.AnchorType.FullscreenMobileOnly) ? (-10) : 0);
					_panel.SetAnchor(rootAnchor.gameObject, left, -10, 10, 10);
					UIUtility.UpdateAnchors(_panel.transform);
				}
				int depth = 0;
				UIBase currentUI = UIBase.CurrentUI;
				if (currentUI != null)
				{
					UIPanel component = currentUI.GetComponent<UIPanel>();
					if (component != null)
					{
						depth = component.depth - 10;
					}
				}
				_panel.depth = depth;
			}
		}
		if (_panel != null)
		{
			_panel.enabled = show;
		}
		_blurTexture.enabled = show;
	}

	private void OnClose(GameObject obj)
	{
		UIBase.CloseAllUI();
	}

	public void SetParameters(float spacing, float vibrancy, Color tintColor)
	{
		bool flag = false;
		if (Math.Abs(_spacing - spacing) > 0.001f)
		{
			_spacing = spacing;
			flag = true;
		}
		if (Math.Abs(_vibrancy - vibrancy) > 0.001f)
		{
			_vibrancy = vibrancy;
			flag = true;
		}
		if (_tintColor != tintColor)
		{
			_tintColor = tintColor;
			flag = true;
		}
		if (flag || !_blurTexture.enabled)
		{
			UpdateBlurTexture();
		}
	}

	private void UpdateBlurTexture()
	{
		_blurTexture.material.SetColor(ShaderTintColor, _tintColor);
		_blurTexture.material.SetFloat(ShaderSize, _spacing);
		_blurTexture.material.SetFloat(ShaderVibrancy, _vibrancy);
		_blurTexture.RemoveFromPanel();
		_blurTexture.MarkAsChanged();
	}
}
