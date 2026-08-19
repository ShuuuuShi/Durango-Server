using UnityEngine;

public class Outline : MeshCloner
{
	[SerializeField]
	private Material _material;

	private Color _startColor;

	private float _currentAlpha;

	private bool _isFading;

	private bool _fadeShow;

	private void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		ModyfyRenderLayer = 11;
		_material = new Material(_material);
		_startColor = _material.GetColor("_OutlineColor");
		base.Show = false;
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)_material);
	}

	private void Update()
	{
		if (!_isFading)
		{
			return;
		}
		float num = Time.deltaTime * 2f;
		if (_fadeShow)
		{
			_currentAlpha += num;
			if (_currentAlpha >= 1f)
			{
				_currentAlpha = 1f;
				_isFading = false;
			}
		}
		else
		{
			_currentAlpha -= num;
			if (_currentAlpha <= 0f)
			{
				_currentAlpha = 0f;
				_isFading = false;
				base.Show = false;
			}
		}
		SetAlpha(_currentAlpha);
	}

	public void Fade(bool show)
	{
		if (_fadeShow != show)
		{
			_isFading = true;
			_fadeShow = show;
			if (_fadeShow)
			{
				base.Show = true;
			}
			SetAlpha(_currentAlpha);
		}
	}

	protected override Material GetSourceMaterial()
	{
		return _material;
	}

	private void SetAlpha(float alpha)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_startColor.a = alpha;
		_material.SetColor("_OutlineColor", _startColor);
	}
}
