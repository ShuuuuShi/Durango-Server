using L10N;
using UnityEngine;

public class ExpGauge : MonoBehaviour
{
	private UIWidget _widget;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _upper;

	[SerializeField]
	private UISprite _fill;

	[SerializeField]
	private UISprite _fillEffect;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private int _labelMargin;

	[SerializeField]
	private int _fillSpeed;

	[LocalizableString]
	[SerializeField]
	private string _labelFormat;

	private int _currentWidth;

	private int _targetWidth;

	private float _fillEffectAlpha;

	private bool _enableFillEffect;

	public UIWidget Widget => _widget;

	public void Show(bool show)
	{
		if (show)
		{
			((Component)this).gameObject.SetActive(true);
			Set(GameSystem<StatisticsSystem>.Instance().Exp, init: false);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void Awake()
	{
		_upper.width = 0;
		_fill.width = 0;
		_currentWidth = -1;
	}

	private void OnEnable()
	{
		GameSystem<StatisticsSystem>.Instance().ExpChanged += OnChangeExp;
	}

	private void OnDisable()
	{
		GameSystem<StatisticsSystem>.Instance().ExpChanged -= OnChangeExp;
	}

	private void OnPortraitMode(bool isPortrait)
	{
		Set(GameSystem<StatisticsSystem>.Instance().Exp, init: true);
	}

	private void OnChangeExp(int prev, int current)
	{
		Set(current, UIManager.IsLoadingCurtain);
	}

	private void Set(int exp, bool init)
	{
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		GameSystem<StatisticsSystem>.Instance().GetExpRange(level, out var min, out var max);
		int num = exp - min;
		int num2 = max - min;
		Set(level, (num2 != 0) ? ((float)num / (float)num2) : 0f, init);
	}

	private void Set(int level, float ratio, bool init)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)((float)_background.width * ratio);
		_fill.width = num;
		_fill.alpha = Mathf.Clamp01((float)num);
		_label.text = T._(_labelFormat, ratio, level);
		float num2 = _label.printedSize.x + (float)(_labelMargin * 2);
		if ((float)num > num2)
		{
			((Component)_label).transform.localPosition = Vector3.right * ((float)num - num2 / 2f);
		}
		else
		{
			((Component)_label).transform.localPosition = Vector3.right * ((float)num + num2 / 2f);
		}
		_targetWidth = num;
		if (init)
		{
			_currentWidth = _targetWidth;
			_upper.width = _targetWidth;
			_upper.alpha = Mathf.Clamp01((float)_currentWidth);
		}
		else
		{
			_enableFillEffect = true;
			((Component)_fillEffect).gameObject.SetActive(true);
		}
	}

	private void Update()
	{
		if (_currentWidth < _targetWidth)
		{
			int num = _targetWidth - _currentWidth;
			int num2 = (int)((float)_fillSpeed * Time.deltaTime);
			if (num < num2)
			{
				_currentWidth = _targetWidth;
			}
			else
			{
				_currentWidth += num2;
			}
			_upper.width = _currentWidth;
			_upper.alpha = Mathf.Clamp01((float)_currentWidth);
			_fillEffectAlpha += Time.deltaTime / 0.2f;
			_fillEffectAlpha = Mathf.Min(_fillEffectAlpha, 1f);
			_fillEffect.alpha = _fillEffectAlpha;
			_fillEffect.width = _currentWidth;
		}
		else if (_currentWidth > _targetWidth)
		{
			_currentWidth = _targetWidth;
			_upper.width = _currentWidth;
			_upper.alpha = Mathf.Clamp01((float)_currentWidth);
			_fillEffect.alpha = 0f;
			_fillEffectAlpha = 0f;
			_enableFillEffect = false;
			((Component)_fillEffect).gameObject.SetActive(false);
		}
		else if (_enableFillEffect)
		{
			if (_fillEffectAlpha > 0f)
			{
				_fillEffectAlpha -= Time.deltaTime / 0.2f;
				_fillEffect.alpha = _fillEffectAlpha;
				return;
			}
			_fillEffect.alpha = 0f;
			_fillEffectAlpha = 0f;
			_enableFillEffect = false;
			((Component)_fillEffect).gameObject.SetActive(false);
		}
	}
}
