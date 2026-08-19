using UnityEngine;

namespace Durango.Render;

public class Outline : VisibleObject
{
	private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

	private static readonly int OutlineWidth = Shader.PropertyToID("_Outline");

	[SerializeField]
	private Material _material;

	private int _defaultRendererQueue;

	private Color _color;

	private float _currentAlpha;

	private float _targetAlpha;

	private float CurrentAlpha
	{
		get
		{
			return _currentAlpha;
		}
		set
		{
			_currentAlpha = value;
			ApplyColor();
			MeshCloner.SetVisible(_currentAlpha > 0f);
			if (Mathf.Approximately(CurrentAlpha, _targetAlpha))
			{
				base.enabled = false;
			}
		}
	}

	private float TargetAlpha
	{
		get
		{
			return _targetAlpha;
		}
		set
		{
			_targetAlpha = value;
			if (!Mathf.Approximately(CurrentAlpha, _targetAlpha))
			{
				base.enabled = true;
			}
		}
	}

	protected new void Awake()
	{
		base.Awake();
		MeshCloner.OverrideRenderLayer(11);
		_material = new Material(_material);
		_color = _material.GetColor(OutlineColor);
		_defaultRendererQueue = _material.renderQueue;
		SetVisible(visible: false);
		SkipFade();
	}

	public override void Add(SkinnedMeshRenderer[] renderers)
	{
		MeshCloner.Add(base.transform, renderers, _material);
	}

	public override void Remove(SkinnedMeshRenderer[] renderers)
	{
		MeshCloner.Remove(renderers);
	}

	public void SkipFade()
	{
		CurrentAlpha = TargetAlpha;
	}

	protected override void OnVisibleChanged(bool visible)
	{
		TargetAlpha = ((!visible) ? 0f : 1f);
	}

	private void OnDestroy()
	{
		Object.Destroy(_material);
	}

	private void Update()
	{
		float maxDelta = Time.deltaTime * 2f;
		CurrentAlpha = Mathf.MoveTowards(CurrentAlpha, TargetAlpha, maxDelta);
	}

	public void SetColor(Color color)
	{
		_color = color;
		ApplyColor();
	}

	private void ApplyColor()
	{
		_color.a = CurrentAlpha;
		_material.SetColor(OutlineColor, _color);
	}

	public void SetWidth(float width)
	{
		_material.SetFloat(OutlineWidth, width);
	}

	public void SetRendererQueueOffset(int offset)
	{
		_material.renderQueue = _defaultRendererQueue + offset;
	}
}
