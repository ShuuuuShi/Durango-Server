using System.Collections.Generic;
using EnvironmentData;
using FatigueData;
using JetBrains.Annotations;
using TimerData;
using UnityEngine;

public class FatigueWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _fatigueLabel;

	[SerializeField]
	private UIWidget _fatigueGauge;

	[SerializeField]
	private UILabel _gaugeMinLabel;

	[SerializeField]
	private UILabel _gaugeMaxLabel;

	[SerializeField]
	private ListObjectPool _momentumWidget;

	[SerializeField]
	private UIWidget _upperSprite;

	[SerializeField]
	private UISprite _guildLine;

	[SerializeField]
	private UIWidget _warningBar;

	[SerializeField]
	private UILabel _warningTimer;

	[SerializeField]
	private UILabel _dangerTimer;

	[SerializeField]
	private UILabel _fatigueVelocity;

	[SerializeField]
	private UILabel _fatigueVelocityPeriod;

	private float _timerUpdateTimer;

	private float[] _momentumsRatio;

	[CanBeNull]
	private Fatigue _fatigue;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Update()
	{
		if (_fatigue != null)
		{
			if (_timerUpdateTimer > 0f)
			{
				_timerUpdateTimer -= Time.deltaTime;
				return;
			}
			_timerUpdateTimer = 1f;
			UpdateTimer();
			UpdateFatigueGuage();
		}
	}

	public void Set(Fatigue fatigue, List<FatigueVelocity> velocities)
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		if (fatigue == null)
		{
			return;
		}
		_fatigue = fatigue;
		UpdateFatigueCaption();
		float velocity = _fatigue.Velocity;
		float num = Mathf.Abs(velocity * 60f);
		_fatigueVelocityPeriod.text = LocalizeSystem.Get((!(velocity < 0f)) ? "#fatigue_gauge_increase_per_min" : "#fatigue_gauge_decrease_per_min");
		_fatigueVelocity.text = string.Format("{1}{0:0.#}", num, (!(velocity < 0f)) ? "+" : "-");
		_gaugeMinLabel.text = "0";
		_gaugeMaxLabel.text = _fatigue.Max.ToString("0");
		_momentumWidget.Set(velocities.Count);
		_momentumsRatio = new float[velocities.Count];
		for (int i = 0; i < velocities.Count; i++)
		{
			FatigueVelocity fatigueVelocity = velocities[i];
			_momentumsRatio[i] = ((velocity != 0f) ? Mathf.Clamp01(fatigueVelocity.Value / velocity) : 0f);
			SimpleContainer component = _momentumWidget[i].GetComponent<SimpleContainer>();
			UISprite uISprite = component.Get<UISprite>("bg");
			UISprite uISprite2 = component.Get<UISprite>("icon");
			if (fatigueVelocity.CategoryData == null)
			{
				Debug.LogError((object)("No data for fatigue category : " + fatigueVelocity.Category));
				continue;
			}
			uISprite2.spriteName = fatigueVelocity.CategoryData.icon;
			uISprite.color = fatigueVelocity.CategoryData.GetColor();
		}
		UpdateTimer();
		UpdateFatigueGuage();
		_timerUpdateTimer = 1f;
	}

	private void UpdateFatigueCaption()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		if (_fatigue != null)
		{
			Vector3 localPosition = ((Component)_warningBar).transform.localPosition;
			localPosition.x = (float)_fatigueGauge.width * _fatigue.GetRatio(_fatigue.Warning);
			((Component)_warningBar).transform.localPosition = localPosition;
			Vector3 localPosition2 = ((Component)_warningTimer).transform.parent.localPosition;
			localPosition2.x = (float)(-_fatigueGauge.width) / 2f + ((Component)_warningBar).transform.localPosition.x;
			((Component)_warningTimer).transform.parent.localPosition = localPosition2;
			UISpriteData atlasSprite = _guildLine.GetAtlasSprite();
			int width = atlasSprite.width;
			int width2 = _fatigueGauge.width;
			float num = (float)width2 / (float)width;
			float ratio = _fatigue.GetRatio(25f);
			num *= ratio;
			_guildLine.width = (int)((float)width2 / num);
			((Component)_guildLine).transform.localScale = new Vector3(num, 1f, 1f);
		}
	}

	private void UpdateTimer()
	{
		if (_fatigue != null)
		{
			float num = _fatigue.Remain(_fatigue.Warning);
			float num2 = _fatigue.Remain(_fatigue.Max);
			string text = null;
			string text2 = null;
			float velocity = _fatigue.Velocity;
			if (velocity > 0.01f)
			{
				text = TimerSystem.TimeToString(num, TimePeriod.Min, 2);
				text2 = TimerSystem.TimeToString(num2, TimePeriod.Min, 2);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = LocalizeSystem.Get((!(num > 0f)) ? "#fatigue_warning_state" : "#fatigue_soon_warning_state");
			}
			_warningTimer.text = text;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = LocalizeSystem.Get((!(num2 > 0f)) ? "#fatigue_danger_state" : "#fatigue_soon_danger_state");
			}
			_dangerTimer.text = text2;
		}
	}

	private void UpdateFatigueGuage()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (_fatigue != null)
		{
			float num = _fatigue.Get();
			_fatigueLabel.text = $"[ffd85b]{num:0}[-] [797979]/[-] {_fatigue.Max:0}";
			float ratio = _fatigue.GetRatio(num);
			float num2 = ((float)_fatigueGauge.width - 4f) * ratio;
			Vector3 val = _momentumWidget.BaseObject.transform.localPosition;
			int i = 0;
			for (int count = _momentumWidget.Count; i < count; i++)
			{
				SimpleContainer component = _momentumWidget[i].GetComponent<SimpleContainer>();
				UISprite uISprite = component.Get<UISprite>("icon");
				UIWidget uIWidget = component.Get<UIWidget>((string)null);
				float num3 = num2 * _momentumsRatio[i];
				uIWidget.width = (int)num3 + 4;
				((Component)uISprite).transform.localPosition = Vector3.right * (float)uIWidget.width / 2f;
				((Component)uISprite).gameObject.SetActive(num3 > (float)(uISprite.width + 10));
				((Component)uIWidget).transform.localPosition = val;
				val += Vector3.right * num3;
			}
			_upperSprite.alpha = ratio;
		}
	}
}
