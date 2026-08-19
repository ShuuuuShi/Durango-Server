using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class PointTargetWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _portraitWidget;

	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private UISprite _seasonSprite;

	[SerializeField]
	private UISprite _borderSprite;

	[SerializeField]
	private UISprite _distanceBgSprite;

	[SerializeField]
	private UILabel _distanceLabel;

	[SerializeField]
	private GameObject _directionObject;

	[SerializeField]
	private UISprite _directionSprite;

	[SerializeField]
	private UISprite _underArrowSprite;

	[SerializeField]
	private UISprite _shadowSprite;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private TweenPosition _portraitTween;

	[SerializeField]
	private TweenScale _shadowTween;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private float _moveGuideDistance = 37f;

	[SerializeField]
	private UISprite _gauge;

	[SerializeField]
	private UISprite _warning;

	private int _lastDistance;

	private int _depth;

	private bool _selected;

	private bool _playingTweens;

	private Color _defaultBorderColor;

	private PointTargetController.Arguments? _arguments;

	private UIPanel _panel;

	public string Key { get; private set; }

	private void Awake()
	{
		_portraitTween.method = UITweener.Method.EaseInOut;
		_shadowTween.method = UITweener.Method.EaseInOut;
		_panel = GetComponent<UIPanel>();
	}

	public bool Tick()
	{
		PointTargetController.Arguments? arguments = _arguments;
		if (!arguments.HasValue)
		{
			return false;
		}
		PointTargetController.Arguments value = _arguments.Value;
		if (!value.TryGetPosition(out var pos))
		{
			return false;
		}
		Vector3 vector = pos - PlayerBehavior.LocalPlayer.CurrentPosition;
		int num = Mathf.CeilToInt(vector.magnitude / 100f);
		if (num != _lastDistance)
		{
			UpdateDistance(num);
		}
		Vector3 world = pos;
		if (vector.sqrMagnitude > 10240000f)
		{
			vector.Normalize();
			world = PlayerBehavior.LocalPlayer.CurrentPosition + vector * 3200f;
		}
		Vector3 vector2 = MainCamera.WorldToNGUIPos(world);
		Vector3 zero = Vector3.zero;
		Vector2 vector3 = vector2 - zero;
		float num2 = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
		Vector3 vector4 = Singleton<ScreenAreaManager>.Instance().GetBorder(num2 - 1f);
		Vector3 vector5 = Singleton<ScreenAreaManager>.Instance().GetBorder(num2 + 1f);
		Vector3 normalized = (vector5 - vector4).normalized;
		normalized = new Vector3(normalized.y, 0f - normalized.x);
		Vector3 vector6 = Singleton<ScreenAreaManager>.Instance().GetBorder(num2);
		vector6 -= normalized * 50f;
		Vector2 vector7 = vector6 - zero;
		if (float.IsNaN(vector6.x) || float.IsNaN(vector6.y))
		{
			return false;
		}
		if (vector3.sqrMagnitude < vector7.sqrMagnitude)
		{
			if (value.HideInScreen)
			{
				_panel.alpha = 0f;
			}
			else
			{
				UpdateSprite(withInScreen: true);
				base.transform.localPosition = vector2;
			}
		}
		else
		{
			if (value.HideInScreen)
			{
				_panel.alpha = 1f;
			}
			base.transform.localPosition = vector6;
			UpdateSprite(withInScreen: false, num2);
		}
		return true;
	}

	public void Clear()
	{
		Key = null;
		_arguments = null;
		_lastDistance = 0;
	}

	public void SetTarget(string key, PointTargetController.Arguments args)
	{
		bool flag = Key != key;
		Key = key;
		_arguments = args;
		int valueOrDefault = args.IconSize.GetValueOrDefault(Mathf.Min(_portraitWidget.width, _portraitWidget.height));
		Color valueOrDefault2 = args.BorderColor.GetValueOrDefault(Color.white);
		_portraitSprite.spriteName = args.Icon;
		_portraitSprite.color = args.IconColor.GetValueOrDefault(Color.white);
		_defaultBorderColor = valueOrDefault2;
		_borderSprite.color = valueOrDefault2;
		_directionSprite.color = valueOrDefault2;
		_underArrowSprite.color = valueOrDefault2;
		_shadowSprite.color = valueOrDefault2;
		UIUtility.ResizeToSquare(_portraitSprite, valueOrDefault);
		SeasonUtil.SetSmallIcon(_seasonSprite, args.Season);
		_gauge.gameObject.SetActive(value: false);
		_warning.gameObject.SetActive(value: false);
		_bgSprite.gameObject.SetActive(args.ShowBg);
		if (flag)
		{
			PlayMakeEffect();
		}
	}

	public void SetDepth(int depth)
	{
		_depth = depth;
		UpdateDepth();
	}

	public void Select(bool selected)
	{
		_selected = selected;
		Color color = ((!_selected) ? _defaultBorderColor : PresetColor.UIYellow);
		_borderSprite.color = color;
		_directionSprite.color = color;
		_underArrowSprite.color = color;
		_shadowSprite.color = color;
		Vector3 vector = Vector3.one * ((!_selected) ? 1f : 1.2f);
		_portraitWidget.transform.localScale = vector;
		_portraitWidget.GetComponent<TweenScale>().to = vector;
		UpdateDepth();
	}

	public void UpdateGauge(float value, bool warning)
	{
		_gauge.gameObject.SetActive(value: true);
		_gauge.fillAmount = value;
		_gauge.color = ((!warning) ? new Color32(99, 142, 73, byte.MaxValue) : new Color32(186, 46, 46, byte.MaxValue));
		_warning.gameObject.SetActive(warning);
		if (warning)
		{
			_gauge.GetComponent<TweenAlpha>().PlayForward();
		}
	}

	private void UpdateDepth()
	{
		int num = _depth + 10;
		if (_selected)
		{
			num += 30;
		}
		_panel.depth = num;
	}

	private void UpdateSprite(bool withInScreen, float degree = 0f)
	{
		_distanceBgSprite.gameObject.SetActive(!withInScreen);
		_directionObject.gameObject.SetActive(!withInScreen);
		_underArrowSprite.gameObject.SetActive(withInScreen);
		_shadowSprite.gameObject.SetActive(withInScreen);
		if (withInScreen)
		{
			PlayAllTweens();
			return;
		}
		_directionObject.transform.localRotation = Quaternion.Euler(0f, 0f, degree);
		Vector3 localPosition = _distanceBgSprite.transform.localPosition;
		if (210f < degree && degree < 330f)
		{
			localPosition.y = _moveGuideDistance;
		}
		else
		{
			localPosition.y = 0f - _moveGuideDistance;
		}
		_distanceBgSprite.transform.localPosition = localPosition;
		StopAllTweens();
		_portraitWidget.transform.localPosition = Vector3.zero;
	}

	private void UpdateDistance(int distance)
	{
		_lastDistance = distance;
		_distanceLabel.text = $"{distance} m";
	}

	private void PlayAllTweens()
	{
		if (!_playingTweens)
		{
			_portraitTween.tweenFactor = 0f;
			_portraitTween.PlayForward();
			_shadowTween.tweenFactor = 0f;
			_shadowTween.PlayForward();
			_playingTweens = true;
		}
	}

	private void StopAllTweens()
	{
		if (_playingTweens)
		{
			_portraitTween.transform.localPosition = Vector3.zero;
			_portraitTween.enabled = false;
			_shadowTween.enabled = false;
			_playingTweens = false;
		}
	}

	private void PlayMakeEffect()
	{
		if (!UIManager.IsLoadingCurtain)
		{
			_tweenerPlayer.Play();
		}
	}
}
