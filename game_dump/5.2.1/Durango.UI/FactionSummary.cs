using System;
using Durango.Logic.Faction;
using Durango.Logic.Notification;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionSummary : UIWidget
{
	[SerializeField]
	private GameObject _mainContainer;

	[SerializeField]
	private UILabel _talkLabel;

	[SerializeField]
	private GameObject _talkNotification;

	[SerializeField]
	private GameObject _talksButton;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISprite _seasonEmblemSprite;

	[SerializeField]
	private UILabel _factionNameLabel;

	[SerializeField]
	private UISprite _gaugeUpperSprite;

	[SerializeField]
	private UILabel _gaugeValueLabel;

	[SerializeField]
	private UILabel _factionGradeLabel;

	[SerializeField]
	private UILabel _supportTimerLabel;

	[SerializeField]
	private SelectableButton _supportButton;

	[SerializeField]
	private UISprite _supportNotification;

	[SerializeField]
	private GameObject _unknownContainer;

	[SerializeField]
	private UILabel _unknownLabel;

	[SerializeField]
	private RectLayoutComponent _layout;

	private bool _isInit;

	private static readonly int FilterTex = Shader.PropertyToID("_FilterTex");

	private Material _portraitMaterial;

	public Durango.Logic.Faction.Faction Faction { get; private set; }

	public SelectableButton SupportButton => _supportButton;

	public event Action<FactionSummary> TalksClicked;

	public event Action<FactionSummary> SupportRequestClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		UIEventListener uIEventListener = UIEventListener.Get(_talksButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.TalksClicked != null)
			{
				this.TalksClicked(this);
			}
		});
		SelectableButton supportButton = _supportButton;
		supportButton.Clicked = (Action)Delegate.Combine(supportButton.Clicked, (Action)delegate
		{
			if (this.SupportRequestClicked != null)
			{
				this.SupportRequestClicked(this);
			}
		});
	}

	public void UpdateLayout(Point2 size)
	{
		SetDimensions(size.x, size.y);
		_layout.UpdateLayout(size.x, size.y);
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Set(Durango.Logic.Faction.Faction faction, Material portrait, Rect portraitUv, string unknownText)
	{
		Init();
		Faction = faction;
		if (faction.IsAvailable())
		{
			_mainContainer.gameObject.SetActive(value: true);
			_unknownContainer.gameObject.SetActive(value: false);
			FillFactionInfo();
			FillPortraitInfo(portrait, portraitUv);
			FillTalksInfo();
			FillSupportReuqestInfo();
			CheckNotification();
		}
		else
		{
			_mainContainer.gameObject.SetActive(value: false);
			_unknownContainer.gameObject.SetActive(value: true);
			_unknownLabel.text = unknownText;
		}
	}

	private void CheckNotification()
	{
		_talkNotification.gameObject.SetActive(Faction.GetTalkNotification());
		bool active = Faction.HasAvailableSupportRequest();
		_supportNotification.gameObject.SetActive(active);
		_supportNotification.color = Notification.GetTypeColor(Durango.Logic.Notification.Type.Normal);
	}

	private void FillTalksInfo()
	{
		Talks[] array = SingletonDict<FactionType, Talks[]>.Instance.Get(Faction.Type);
		int num = -1;
		for (int num2 = KUtility.GetSize(array) - 1; num2 >= 0; num2--)
		{
			if (array[num2].FriendshipPoint <= Faction.Point)
			{
				num = num2;
				break;
			}
		}
		if (num == -1 || KUtility.GetSize(array[num].List) == 0)
		{
			_talkLabel.text = string.Empty;
		}
		else
		{
			_talkLabel.text = array[num].List[0].Message;
		}
	}

	private void FillFactionInfo()
	{
		Faction.GetFactionGaugeValues(out var current, out var max);
		float num = ((!((float)max > 0f)) ? 0f : ((float)current / (float)max));
		_gaugeUpperSprite.fillAmount = num;
		_gaugeUpperSprite.alpha = ((!(num > 0f)) ? 0f : 1f);
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(Faction.Type);
		if (faction != null)
		{
			_factionNameLabel.text = faction.Name;
			_factionGradeLabel.text = $"[icon=faction_amity] {faction.Titles.Get<Gettext>(Faction.Level - 1, string.Empty)}";
		}
		_gaugeValueLabel.text = $"{current} / {max}";
	}

	private void FillPortraitInfo(Material portrait, Rect portraitUv)
	{
		if (_portraitMaterial == null)
		{
			_portraitMaterial = UnityEngine.Object.Instantiate(portrait);
		}
		else
		{
			_portraitMaterial.CopyPropertiesFromMaterial(portrait);
		}
		if (_portraitMaterial.GetTexture(FilterTex) != null)
		{
			Vector2 value = new Vector2(1f / portraitUv.size.x, 1f / portraitUv.size.y);
			Vector2 value2 = new Vector2(1f - portraitUv.position.x * value.x, 1f - portraitUv.position.y * value.y);
			_portraitMaterial.SetTextureOffset(FilterTex, value2);
			_portraitMaterial.SetTextureScale(FilterTex, value);
		}
		_portraitTexture.material = _portraitMaterial;
		_portraitTexture.uvRect = portraitUv;
		Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(Faction.Type);
		if (faction != null)
		{
			SeasonUtil.SetLargeIcon(_seasonEmblemSprite, faction.Season);
		}
	}

	private void FillSupportReuqestInfo()
	{
		_supportTimerLabel.alignment = (UIManager.IsPortraitWidget(base.gameObject) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
		if (Faction.HasSupportRequest())
		{
			if (Faction.EndsAt > 0.0)
			{
				_supportTimerLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					double num = Faction.EndsAt - Connections.Frontend.GetPredictedServerTime();
					if (num > 0.0)
					{
						text = T._("{0} 남음", TimedeltaFormatter.Format(num, 1, "min"));
						period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
					}
					else
					{
						text = string.Empty;
						period = 0f;
					}
				}));
			}
			else if (Faction.HasAvailableSupportRequest())
			{
				_supportTimerLabel.SetText(T._("지원 가능!"));
				_supportTimerLabel.SetEnable<UITweener>(enable: false);
				_supportTimerLabel.alpha = 1f;
			}
			else
			{
				_supportTimerLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					SyncString.UpdateRemainTimeMsg(Faction.SupportRequestAvailableAt, "[icon=icon_timer] {0}", out text, out period, string.Empty);
				}));
				_supportTimerLabel.SetEnable<UITweener>(enable: false);
				_supportTimerLabel.alpha = 1f;
			}
			_supportTimerLabel.transform.parent.gameObject.SetActive(value: true);
			_supportButton.Text = T._("지원 목록");
			_supportButton.Disabled = false;
		}
		else
		{
			_supportTimerLabel.transform.parent.gameObject.SetActive(value: false);
			_supportButton.Text = T._("지원 불가");
			_supportButton.Disabled = true;
		}
	}
}
