using System;
using System.Collections.Generic;
using Building;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public abstract class InteractionMenuWidgetBase : SelectableWidget
{
	[SerializeField]
	protected ProgressGauge ProgressGauge;

	[SerializeField]
	protected ItemIconTex IconTexture;

	[SerializeField]
	protected UIWidget TextWidget;

	[SerializeField]
	protected UILabel NameLabel;

	[SerializeField]
	protected UILabel InfoLabel;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UILabel _timeLabel;

	[SerializeField]
	private float _alphaBgDisabled;

	private float _alpha;

	private TweenPosition _positionTweener;

	private TweenAlpha _alphaTweener;

	[SerializeField]
	private string _criticalImageName;

	public float MenuRadian { get; set; }

	public bool Valid { get; set; }

	public bool Empty { get; set; }

	public InteractionMenuData Data { get; private set; }

	public MenuType Type { get; private set; }

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
			if (AlphaTweener.enabled)
			{
				AlphaTweener.to = value;
			}
			else
			{
				base.Widget.alpha = value;
			}
		}
	}

	protected string Description
	{
		get
		{
			return _description.text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				_description.gameObject.SetActive(value: true);
				_description.text = value;
			}
			else
			{
				_description.gameObject.SetActive(value: false);
				_description.text = string.Empty;
			}
		}
	}

	protected string Name
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				NameLabel.gameObject.SetActive(value: false);
				return;
			}
			NameLabel.gameObject.SetActive(value: true);
			NameLabel.text = value;
		}
	}

	public bool NeedInitAnimation { get; set; }

	public TweenPosition PositionTweener
	{
		get
		{
			if (_positionTweener == null)
			{
				_positionTweener = GetComponent<TweenPosition>();
			}
			return _positionTweener;
		}
	}

	public TweenAlpha AlphaTweener
	{
		get
		{
			if (_alphaTweener == null)
			{
				_alphaTweener = GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public int Index { get; set; }

	public abstract bool IsWarning();

	private static string GetTimeString(float time)
	{
		if (time < 0f)
		{
			return string.Empty;
		}
		if (time == 0f)
		{
			return T._("-초");
		}
		if (time < 10f)
		{
			return T._("{0:n1}초", time);
		}
		return TimedeltaFormatter.Format(time);
	}

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.InteractionMenuDefault;
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		RefreshIconTextureColor();
		if (!base.IsChangeSelected)
		{
			return;
		}
		float alpha = ((!base.Selected) ? 1f : 0f);
		if (base.gameObject.activeInHierarchy)
		{
			TweenAlpha.Begin(TextWidget.gameObject, 0.2f, alpha);
			return;
		}
		TweenAlpha component = TextWidget.GetComponent<TweenAlpha>();
		if (component != null)
		{
			component.enabled = false;
		}
		TextWidget.alpha = alpha;
	}

	protected virtual void RefreshIconTextureColor()
	{
		if (base.IsChangePressed)
		{
			IconTexture.color = ((!base.Pressed) ? Data.Color : PresetColor.UIYellow);
		}
	}

	public void Set(InteractionMenuData data, InteractionObject target)
	{
		base.Disabled = false;
		Data = data;
		Valid = true;
		Empty = false;
		string text2 = null;
		bool emphasis = false;
		string text3 = null;
		Type = InteractionMenuPriority.GetAttribute(Data.Action).Type;
		base.transform.localScale = Vector3.one * ((Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
		Name = Data.Name;
		IconTexture.SetIcon(Data.Icon);
		Description = ((Data.Count <= 0) ? string.Empty : Data.Count.ToString());
		IconTexture.color = Data.Color;
		if (Data.GatheringData != null)
		{
			SetDurationText(Data.GatheringData.Duration);
			if (Data.GatheringData.IsAvailableForGathering())
			{
				Alpha = 1f;
			}
			else
			{
				bool num = Data.GatheringData.RequiredTools.Count > 0;
				Alpha = _alphaBgDisabled;
				if (num)
				{
					text3 = "[icon=img_notool]";
				}
			}
			if (Data.GatheringData.IsCritical)
			{
				text2 = "[icon=" + _criticalImageName + "]";
				emphasis = Data.GatheringData.Amount == 1;
			}
		}
		else
		{
			if (Data.Action == Interaction.Warp && Data.Disabled)
			{
				Alpha = _alphaBgDisabled;
				text3 = "[icon=img_warpnono]";
			}
			else if (Data.Action == Interaction.RideBalloon)
			{
				string voucherId = Singleton<CostsYaml>.Instance.BalloonTicket.VoucherId;
				int voucherCount = InventorySystem.Wallet.GetVoucherCount(voucherId);
				if (voucherCount > 0)
				{
					Voucher voucher = SingletonDict<string, Voucher>.Get(voucherId);
					voucher.IsValid();
					Alpha = _alphaBgDisabled;
					text2 = $"{voucher.GetIconText()} {voucherCount}";
				}
				else
				{
					Alpha = 1f;
				}
			}
			else if (Data.Action == Interaction.RemodelArtifact)
			{
				Artifact artifact = target?.GetTargetComponent<Artifact>();
				Building.Blueprint blueprint = ((!(artifact != null)) ? null : artifact.Blueprint);
				if (blueprint != null && blueprint.Available)
				{
					Alpha = 1f;
				}
				else
				{
					Alpha = _alphaBgDisabled;
					text3 = "[icon=img_notool]";
				}
			}
			else if (Data.Action == Interaction.ExtendFloor && Data.Disabled)
			{
				Alpha = _alphaBgDisabled;
				text3 = "[icon=img_notool]";
			}
			else
			{
				Alpha = 1f;
			}
			SetDurationText(Data.Duration);
		}
		SetInfoText(text2, emphasis);
		SetWaringText(text3, emphasis: false);
		if (Data.Timer != null)
		{
			ProgressGauge.Play(Data.Timer);
			if (NeedSyncTimeLabel(Data.Timer))
			{
				_timeLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					if (Data.Timer == null)
					{
						period = 0f;
						text = null;
					}
					else
					{
						float remain = Data.Timer.Remain;
						text = GetDurationText(remain);
						if (remain > 10f)
						{
							period = remain % (float)TimedeltaFormatter.CurrentMinUnit();
						}
						else
						{
							period = remain % 0.1f;
						}
					}
				}));
			}
			else if (Data.GatheringData != null)
			{
				Data.GatheringData.DurationChanged = SetDurationText;
			}
		}
		else
		{
			ProgressGauge.Timer = null;
		}
		OnSet();
	}

	protected virtual void OnSet()
	{
	}

	private static bool NeedSyncTimeLabel(Timer timer)
	{
		if (timer.Subject.TryEnum<Interaction>(out var value))
		{
			if (value != Interaction.Collect)
			{
				return value != Interaction.Sprinkle;
			}
			return false;
		}
		return true;
	}

	protected abstract void SetWaringText(string text, bool emphasis);

	protected void SetInfoText(string text, bool emphasis)
	{
		if (string.IsNullOrEmpty(text))
		{
			InfoLabel.gameObject.SetActive(value: false);
			return;
		}
		InfoLabel.gameObject.SetActive(value: true);
		InfoLabel.text = text;
		InfoLabel.SetEnable<UITweener>(emphasis);
	}

	public abstract void UpdateUIPosition();

	protected void SetDurationText(float duration)
	{
		if (duration < 0f)
		{
			_timeLabel.gameObject.SetActive(value: false);
			return;
		}
		_timeLabel.gameObject.SetActive(value: true);
		_timeLabel.text = GetDurationText(duration);
	}

	private string GetDurationText(float duration)
	{
		if (duration < 0f)
		{
			return null;
		}
		float num = duration;
		bool num2 = num < 10f;
		if (num2)
		{
			num *= 10f;
		}
		num = Mathf.CeilToInt(num);
		if (num2)
		{
			num *= 0.1f;
		}
		return GetTimeString(num);
	}

	public abstract void SetReservedQueueList(List<Pair<int, ItemIcon>> items);

	public virtual void ClearReservedQueueList()
	{
	}

	public void RemoveFirstQueue()
	{
		GameSystem<InteractionSystem>.Instance().ReservationQueue.RemoveFirst(Data.Action, Data.Id);
	}

	public int GetSign()
	{
		if (MenuRadian > (float)Math.PI / 2f && MenuRadian < 4.712389f)
		{
			return -1;
		}
		return 1;
	}
}
