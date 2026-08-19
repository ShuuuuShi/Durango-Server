using System;
using System.Collections;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MissionFactionNode : SelectableWidget
{
	[SerializeField]
	private GameObject _infoContainer;

	[SerializeField]
	private GameObject _unknownContainer;

	[SerializeField]
	private GameObject _infoesObject;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private UIWidget _contentWidget;

	[SerializeField]
	private UITexture _portraitWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UILabel _unknownLabel;

	[SerializeField]
	private UILabel _hasntMissionLabel;

	[SerializeField]
	private GameObject _doingLabel;

	[SerializeField]
	private UITexture _noiseTexture;

	[SerializeField]
	private Texture2D _noiseTexture1;

	[SerializeField]
	private Texture2D _noiseTexture2;

	[SerializeField]
	private MissionBonusInfoWidget _bonusWidget;

	public const string NoiseSoundEffect = "ui_mission_stop";

	private Material _portrait;

	private string _commentText;

	private float _nextTimerUpdateAt;

	private Mission _mission;

	private double _availableAt;

	private string _commentSubColor;

	private int _saturationId;

	private ICoroutineBinder _noiseCoroutine;

	public bool MissionWidgetOpened { get; set; }

	public FactionType Type { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		Color color = _commentLabel.color;
		color.a *= 0.7f;
		_commentSubColor = NGUIText.EncodeColor32(color);
		_noiseTexture.gameObject.SetActive(value: false);
		_saturationId = Shader.PropertyToID("_Saturation");
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		if (base.IsChangeSelected && _portrait != null)
		{
			float value = ((state != State.Selected) ? 0f : 1f);
			_portrait.SetFloat(_saturationId, value);
			if ((bool)_portraitWidget.drawCall && (bool)_portraitWidget.drawCall.dynamicMaterial)
			{
				_portraitWidget.drawCall.dynamicMaterial.SetFloat(_saturationId, value);
			}
		}
	}

	private void Update()
	{
		if (_nextTimerUpdateAt > 0f && _nextTimerUpdateAt < Time.time)
		{
			UpdateTimerLabel();
		}
	}

	public void SetFactionType(FactionType type, Material portrait, Rect uv)
	{
		Type = type;
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(type);
		_titleLabel.text = $"[icon={IconMap.Get(type)}:1.3]  {faction.Name}";
		if (portrait != null)
		{
			if (_portrait == null)
			{
				_portrait = new Material(portrait);
			}
			else
			{
				_portrait.CopyPropertiesFromMaterial(portrait);
			}
			_portrait.SetFloat(_saturationId, 0f);
			_portraitWidget.material = _portrait;
		}
		else
		{
			_portraitWidget.material = null;
		}
		_portraitWidget.uvRect = uv;
		_unknownLabel.text = faction.UnknownText;
	}

	public void UpdateLayout()
	{
		UIWidget widget = base.Widget;
		_mainWidget.SetDimensions(widget.width - (int)((float)widget.height * 0.8f), widget.height);
		_portraitWidget.SetDimensions(widget.height, widget.height);
		Vector3[] localCorners = widget.localCorners;
		_mainWidget.SetPosition(localCorners[0], 0f, 0f);
		_portraitWidget.SetPosition(localCorners[3], 1f, 0f);
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Set(Mission mission)
	{
		string id = _mission.Id;
		_mission = mission;
		if (id == mission.Id)
		{
			UpdateMission(mission);
			return;
		}
		TryChangeNoise(delegate
		{
			UpdateMission(mission);
		});
	}

	public void SetCooltime(double availableAt)
	{
		if (!string.IsNullOrEmpty(_mission.Id))
		{
			_mission = default(Mission);
			TryChangeNoise(delegate
			{
				UpdateCooltime(availableAt);
			});
		}
		else
		{
			UpdateCooltime(availableAt);
		}
	}

	public void SetHasntMission(string text)
	{
		_mission = default(Mission);
		_availableAt = 0.0;
		_nextTimerUpdateAt = 0f;
		TryChangeNoise(delegate
		{
			_infoContainer.gameObject.SetActive(value: true);
			_unknownContainer.gameObject.SetActive(value: false);
			_doingLabel.gameObject.SetActive(value: false);
			_infoesObject.SetActive(value: false);
			_hasntMissionLabel.gameObject.SetActive(value: true);
			_hasntMissionLabel.text = text;
			_commentLabel.text = string.Empty;
		});
	}

	public void SetUnknown()
	{
		_mission = default(Mission);
		_availableAt = 0.0;
		_nextTimerUpdateAt = 0f;
		_infoContainer.gameObject.SetActive(value: false);
		_unknownContainer.gameObject.SetActive(value: true);
	}

	private void UpdateMission(Mission mission)
	{
		_infoContainer.gameObject.SetActive(value: true);
		_unknownContainer.gameObject.SetActive(value: false);
		_infoesObject.SetActive(value: true);
		_hasntMissionLabel.gameObject.SetActive(value: false);
		_commentText = mission.Subject;
		_infoLabel.text = FactionSystem.MissionRewardToString(mission.Reward);
		_bonusWidget.Set(mission.BonusReward, Type);
		_bonusWidget.gameObject.transform.localPosition = new Vector3(_infoLabel.transform.localPosition.x + _infoLabel.printedSize.x + 10f, 0f, 0f);
		_doingLabel.gameObject.SetActive(mission.StartedAt.HasValue);
		UIUtility.UpdateAnchors(_doingLabel.transform);
		_availableAt = 0.0;
		UpdateTimerLabel();
	}

	private void UpdateCooltime(double availableAt)
	{
		_infoContainer.gameObject.SetActive(value: true);
		_unknownContainer.gameObject.SetActive(value: false);
		_doingLabel.gameObject.SetActive(value: false);
		_infoesObject.SetActive(value: true);
		_hasntMissionLabel.gameObject.SetActive(value: false);
		_availableAt = availableAt;
		_infoLabel.text = "-";
		_bonusWidget.gameObject.SetActive(value: false);
		UpdateTimerLabel();
	}

	private void UpdateTimerLabel()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		_nextTimerUpdateAt = 0f;
		if (_availableAt > 0.0)
		{
			if (_availableAt > predictedServerTime)
			{
				double num = _availableAt - predictedServerTime;
				_commentLabel.text = string.Format("[icon=icon_timer2:0.8] [{2}][size=20]{0}[/size][-]\n{1}", T._("다음 임무까지 남은 시간"), TimedeltaFormatter.Format(num), _commentSubColor);
				_nextTimerUpdateAt = Time.time + (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
			}
			else
			{
				_commentLabel.text = string.Empty;
			}
		}
		else if (_mission.TimeLimit.HasValue)
		{
			if (_mission.StartedAt.HasValue)
			{
				double num2 = _mission.StartedAt.Value + (double)_mission.TimeLimit.Value;
				double num3 = num2 - predictedServerTime;
				if (num3 < 0.0)
				{
					_commentLabel.text = string.Format("{0}\n[icon=icon_timer2:0.8] [{2}][size=20]{1}[-]", _commentText, T._("제한 시간 만료"), _commentSubColor);
					return;
				}
				_commentLabel.text = string.Format("{0}\n[icon=icon_timer2:0.8] [{2}][size=20]{1}[-]", _commentText, TimedeltaFormatter.Format(num3), _commentSubColor);
				_nextTimerUpdateAt = Time.time + (float)(num3 % (double)TimedeltaFormatter.CurrentMinUnit());
			}
			else
			{
				double seconds = _mission.TimeLimit.Value;
				_commentLabel.text = string.Format("{0}\n[icon=icon_timer2:0.8] [{2}][size=20]{1}[-]", _commentText, TimedeltaFormatter.Format(seconds), _commentSubColor);
			}
		}
		else
		{
			_commentLabel.text = _commentText;
		}
	}

	private void TryChangeNoise([NotNull] Action method)
	{
		if (!MissionWidgetOpened)
		{
			MissionWidgetOpened = true;
			method();
		}
		else
		{
			this.StartCoroutine(ref _noiseCoroutine, CoPlayNoiseAction(method));
		}
	}

	private IEnumerator CoPlayNoiseAction([NotNull] Action method)
	{
		_noiseTexture.cachedGameObject.SetActive(value: true);
		_noiseTexture.alpha = 0f;
		SoundManager.PlayEvent("ui_mission_stop");
		for (int i = 0; i < 18; i++)
		{
			if (i >= 0 && i < 8)
			{
				SetNoiseFadeOut(i);
			}
			else if (i >= 8 && i < 16)
			{
				if (i == 8)
				{
					method();
				}
				SetNoiseFadeIn(i - 8);
			}
			if (i >= 4 && i < 18)
			{
				SetNoiseTexture(i - 4);
			}
			yield return new WaitForSeconds(1f / 30f);
		}
		_noiseTexture.cachedGameObject.SetActive(value: false);
		yield return null;
	}

	private void SetNoiseFadeOut(int frame)
	{
		float alpha = 1f - (float)frame * 0.125f;
		_contentWidget.alpha = alpha;
	}

	private void SetNoiseFadeIn(int frame)
	{
		float alpha = (float)frame * 0.125f;
		_contentWidget.alpha = alpha;
	}

	private void SetNoiseTexture(int frame)
	{
		switch (frame % 4)
		{
		case 0:
			_noiseTexture.mainTexture = _noiseTexture1;
			_noiseTexture.cachedTransform.localEulerAngles = Vector3.zero;
			break;
		case 1:
			_noiseTexture.mainTexture = _noiseTexture2;
			_noiseTexture.cachedTransform.localEulerAngles = Vector3.zero;
			break;
		case 2:
			_noiseTexture.mainTexture = _noiseTexture1;
			_noiseTexture.cachedTransform.localEulerAngles = new Vector3(0f, 0f, 180f);
			break;
		case 3:
			_noiseTexture.mainTexture = _noiseTexture2;
			_noiseTexture.cachedTransform.localEulerAngles = new Vector3(0f, 0f, 180f);
			break;
		}
		switch (frame)
		{
		case 0:
		case 12:
			_noiseTexture.alpha = 0.1f;
			break;
		case 1:
		case 11:
			_noiseTexture.alpha = 0.15f;
			break;
		case 13:
			_noiseTexture.alpha = 0.05f;
			break;
		default:
			_noiseTexture.alpha = 0.2f;
			break;
		}
	}
}
