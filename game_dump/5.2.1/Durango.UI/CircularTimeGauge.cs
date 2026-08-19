using System;
using Durango.Environment;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class CircularTimeGauge : MonoBehaviour
{
	[SerializeField]
	private UISprite _gauge;

	[SerializeField]
	private UISprite _gaugeLine;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private GameObject _weatherChangeSprite;

	private WidgetTooltipControl _tooltip;

	private void Awake()
	{
		TimeGauge.IsSunUpChanged += OnSunUpChanged;
		OnSunUpChanged();
		GameSystem<PvpIslandSystem>.Instance().GameStarted += delegate
		{
			base.gameObject.SetActive(value: false);
		};
		WeatherManager weatherManager = Singleton<WeatherManager>.Instance();
		weatherManager.WeatherChanged = (Action<string>)Delegate.Combine(weatherManager.WeatherChanged, (Action<string>)delegate
		{
			if (GameManager.ClusterMode != 0)
			{
				ShowTooltip(5f);
			}
		});
	}

	private void Start()
	{
		_weatherChangeSprite.SetActive(GameManager.ClusterMode != Mode.Online);
	}

	private void Update()
	{
		_gauge.fillAmount = 1f - TimeGauge.GetNormalizedTimeForDayNight();
		_gaugeLine.transform.localRotation = Quaternion.AngleAxis(_gauge.fillAmount * 360f, new Vector3(0f, 0f, 1f));
	}

	private void OnClick()
	{
		if (GameManager.ClusterMode == Mode.Online)
		{
			ShowTooltip(60f);
		}
		else
		{
			ChangeWeather();
		}
	}

	private void OnSunUpChanged()
	{
		_gauge.gameObject.SetActive(value: false);
		_gaugeLine.gameObject.SetActive(value: false);
		_tweenerPlayer.ResetToFirst();
		_tweenerPlayer.Play(forward: true, OnFinishedSunUpChanging, 0f, TimeGauge.IsSunUp ? 1 : 0);
		if (_tooltip != null)
		{
			ShowTooltip(60f);
		}
	}

	private void OnFinishedSunUpChanging()
	{
		_gauge.gameObject.SetActive(value: true);
		_gaugeLine.gameObject.SetActive(value: true);
	}

	private void ShowTooltip(float duration)
	{
		float remainTimeForDayOrNight = TimeGauge.GetRemainTimeForDayOrNight();
		double endAt = Connections.Frontend.GetPredictedServerTime() + (double)remainTimeForDayOrNight;
		string textFormat = GetTooltipTextFormat();
		_tooltip = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		_tooltip.Set(string.Empty, string.Empty, new SyncString(delegate(out string text, out float period)
		{
			SyncString.UpdateRemainTimeMsg(endAt, textFormat, out text, out period, string.Empty);
			_tooltip.MarkAsChanged(TooltipBase.ChangedType.RefreshLayoutAndPosition);
		}));
		_tooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
		_tooltip.Show(_gauge, Vector2.zero, duration);
		_tooltip.AddOnFinished(delegate
		{
			_tooltip = null;
		});
	}

	private static string GetTooltipTextFormat()
	{
		string text;
		string text2;
		string text3;
		if (TimeGauge.IsSunUp)
		{
			text = "icon_sun_02";
			text2 = T._("낮");
			text3 = T._("밤까지 {0} 남음");
		}
		else
		{
			text = "icon_moon_01";
			text2 = T._("밤");
			text3 = T._("낮까지 {0} 남음");
		}
		if (GameManager.ClusterMode == Mode.Online)
		{
			return "<em>[align=0.5][size=28][icon=" + text + "]" + text2 + "[/size][/align]</em>\n" + text3;
		}
		WeatherManager.Weather currentWeather = Singleton<WeatherManager>.Instance().CurrentWeather;
		return "<em>[align=0.5][size=28][icon=" + text + "]" + text2 + "[/size][/align]</em>  [icon=" + currentWeather.GetIcon() + "]" + currentWeather.GetName() + "\n" + text3;
	}

	private void ChangeWeather()
	{
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = "weather"
		});
	}
}
