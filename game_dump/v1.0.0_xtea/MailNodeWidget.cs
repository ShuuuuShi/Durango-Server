using System;
using System.Collections.Generic;
using ClanData;
using ItemSystem;
using L10N;
using MailData;
using Player;
using Shared.Economy;
using Shared.Mailing;
using TimerData;
using UnityEngine;

public class MailNodeWidget : MonoBehaviour
{
	private const float DefaultMargin = 10f;

	public Action<Mail, MailAction> ActionClicked;

	public Action HeightChanged;

	[SerializeField]
	private UISprite _categoryIcon;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private UISpriteLabel _timeLabel;

	[SerializeField]
	private UIWidget _senderContainer;

	[SerializeField]
	private UILabel _senderLabel;

	[SerializeField]
	private ListObjectPool _infoWidgets;

	[SerializeField]
	private ListObjectPool _actionButtons;

	[SerializeField]
	private GameObject _newLabel;

	private List<MailAction> _mailActions = new List<MailAction>();

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

	public Mail Data { get; private set; }

	private PlayerInfo Sender { get; set; }

	public void Init()
	{
		_actionButtons.Init(Init_ActionButtons);
	}

	private void Init_ActionButtons(GameObject obj)
	{
		DefaultSelectableButton component = obj.GetComponent<DefaultSelectableButton>();
		component.Init();
		component.Clicked = OnClick_ActionButton;
	}

	private void OnClick_ActionButton()
	{
		GameObject gameObject = ((Component)Selectable.Current).gameObject;
		int num = _actionButtons.IndexOf(gameObject);
		if (num != -1 && ActionClicked != null)
		{
			ActionClicked(Data, _mailActions[num]);
		}
	}

	public void Set(Mail data)
	{
		Data = data;
		FillStaticData();
		UpdateInfoWidget();
		UpdateActionButton();
		UpdateSender();
		UpdateTimeString();
		UpdateLayout();
	}

	private void FillStaticData()
	{
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		Mail data = Data;
		switch (data.MailType)
		{
		case MailType.Normal:
		{
			string text = data.Text;
			string[] array = text.Split(new char[1] { '\n' }, 2);
			_titleLabel.text = array[0];
			if (array.Length > 1)
			{
				((Component)_commentLabel).gameObject.SetActive(true);
				_commentLabel.text = array[1];
			}
			else
			{
				((Component)_commentLabel).gameObject.SetActive(false);
			}
			_categoryIcon.spriteName = IconMap.Get("mail_type_mail");
			break;
		}
		case MailType.MarketUnregistered:
			_categoryIcon.spriteName = IconMap.Get("mail_type_mail");
			_titleLabel.text = T._("장터에 등록한 아이템이 돌아왔습니다");
			((Component)_commentLabel).gameObject.SetActive(false);
			break;
		case MailType.Invitation:
			_categoryIcon.spriteName = IconMap.Get("mail_type_invite_clan");
			_titleLabel.text = T._("부족: 새로운 부족 가입 요청이 도착했습니다");
			((Component)_commentLabel).gameObject.SetActive(false);
			break;
		default:
			_titleLabel.text = "Not Implemented";
			((Component)_commentLabel).gameObject.SetActive(false);
			break;
		}
		Vector3 localPosition = ((Component)_senderContainer).transform.localPosition;
		localPosition.x = _titleLabel.Label.GetPosition(1f, 1f).x + 15f;
		((Component)_senderContainer).transform.localPosition = localPosition;
		_newLabel.gameObject.SetActive(!data.Accepted);
	}

	private void UpdateInfoWidget()
	{
		Mail data = Data;
		_infoWidgets.Clear();
		foreach (KeyValuePair<Currency, int> item in data.Money)
		{
			MailInfoWidget mailInfoWidget = ((ListObjectPoolBase<GameObject>)_infoWidgets).Add<MailInfoWidget>();
			mailInfoWidget.Set(Inventory.GetIcon(item.Key), item.Value.ToString("N0"));
		}
		int i = 0;
		for (int size = KUtility.GetSize(data.AttachedItems); i < size; i++)
		{
			MailInfoWidget mailInfoWidget2 = ((ListObjectPoolBase<GameObject>)_infoWidgets).Add<MailInfoWidget>();
			mailInfoWidget2.Set(data.AttachedItems[i].Icon, data.AttachedItems[i].Colors, data.AttachedItems[i].Name);
		}
	}

	private void UpdateActionButton()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		Mail data = Data;
		_mailActions.Clear();
		bool flag = data.AttachedItems != null || data.Money.Count > 0;
		if (!data.Accepted && flag)
		{
			_mailActions.Add(MailAction.TakeItems);
		}
		MailType mailType = data.MailType;
		if (mailType == MailType.Invitation && !data.Accepted)
		{
			_mailActions.Add(MailAction.ClanInviteAccept);
			_mailActions.Add(MailAction.ClanInviteReject);
		}
		if (Sender != null && Sender.Valid && Sender.EntityId != GameManager.PlayerId)
		{
			_mailActions.Add(MailAction.ReplyMail);
		}
		if (data.Accepted || !flag)
		{
			_mailActions.Add(MailAction.Delete);
		}
		int count = _mailActions.Count;
		_actionButtons.Set(count);
		for (int i = 0; i < count; i++)
		{
			DefaultSelectableButton component = _actionButtons[i].GetComponent<DefaultSelectableButton>();
			component.Text = LocalizeSystem.Get($"#mail_button_{_mailActions[i]}");
			int width = (int)(component.TextLabel.printedSize.x + 40f);
			component.Widget.width = width;
			UIUtility.UpdateAnchors(((Component)component).transform);
		}
	}

	private void UpdateSender()
	{
		Mail data = Data;
		if (data.SenderId != 0L)
		{
			((Component)_senderContainer).gameObject.SetActive(true);
			_senderLabel.text = string.Empty;
			_senderContainer.alpha = 0f;
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(data.SenderId, Response_MailSenderInfo);
		}
		else
		{
			((Component)_senderContainer).gameObject.SetActive(false);
		}
	}

	private void Response_MailSenderInfo(PlayerInfo playerInfo)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Sender = playerInfo;
		string text = ((!playerInfo.Valid) ? T._("알수없음") : LocalizeSystem.Format("#who_sent", playerInfo.Name));
		_senderLabel.text = text;
		_senderContainer.alpha = 1f;
		_senderContainer.width = (int)(_senderLabel.printedSize.x + ((Component)_senderLabel).transform.localPosition.x * 2f);
		UpdateActionButton();
		UpdateLayout();
	}

	private void Response_ClanInfo(Clan clan)
	{
		_infoWidgets.Set(1);
		MailInfoWidget component = _infoWidgets[0].GetComponent<MailInfoWidget>();
		component.Set(IconMap.Get("mail_invite_clan"), LocalizeSystem.Format("#mail_clan_invite_msg", clan.Name));
		UpdateLayout();
	}

	private void UpdateTimeString()
	{
		double time = Connections.Frontend.GetPredictedServerTime() - Data.SentAt;
		string text = TimerSystem.TimeToString(time, TimePeriod.Min, 1);
		if (string.IsNullOrEmpty(text))
		{
			text = T._("방금");
		}
		_timeLabel.text = string.Format("[{1}] {0}", text, "icon_watch");
	}

	private void UpdateLayout()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		Widget.width = (int)((Component)((Component)this).transform.parent).GetComponent<UIPanel>().width;
		int height = Widget.height;
		float num = ((Component)_commentLabel).transform.localPosition.x;
		float num2 = ((Component)_commentLabel).transform.localPosition.y;
		float num3 = num2 - (float)_actionButtons.BaseObject.GetComponent<UIWidget>().height;
		if (((Component)_commentLabel).gameObject.activeSelf)
		{
			_commentLabel.Label.UpdateNGUIText();
			NGUIText.rectHeight = 10000;
			NGUIText.regionHeight = 10000;
			Vector2 val = NGUIText.CalculatePrintedSize(_commentLabel.text);
			val.y -= (float)_commentLabel.Label.spacingY;
			num2 -= val.y;
			num += val.x;
		}
		int count = _infoWidgets.Count;
		if (count > 0)
		{
			MailInfoWidget component = _infoWidgets.BaseObject.GetComponent<MailInfoWidget>();
			Vector3 localPosition = _infoWidgets.BaseObject.transform.localPosition;
			Vector3 localPosition2 = localPosition;
			localPosition2.y = num2 - 10f;
			float num4 = Widget.localCorners[2].x - 20f;
			float num5 = 0f;
			for (int i = 0; i < count; i++)
			{
				MailInfoWidget component2 = _infoWidgets[i].GetComponent<MailInfoWidget>();
				if (i > 0 && localPosition2.x + (float)component2.Width > num4)
				{
					localPosition2.x = localPosition.x;
					localPosition2.y -= 10f + (float)component.Height;
				}
				((Component)component2).transform.localPosition = localPosition2;
				localPosition2.x += (float)component2.Width + 10f;
				num5 = Mathf.Max(localPosition2.x, num5);
			}
			num2 = localPosition2.y - (float)component.Height;
			num = Mathf.Max(num, num5 + localPosition.x);
		}
		num2 = Mathf.Min(num2, num3);
		int count2 = _actionButtons.Count;
		if (count2 > 0)
		{
			Vector3 zero = Vector3.zero;
			float num6 = 0f;
			for (int j = 0; j < count2; j++)
			{
				if (j > 0)
				{
					num6 += 10f;
				}
				DefaultSelectableButton component3 = _actionButtons[j].GetComponent<DefaultSelectableButton>();
				int width = component3.Widget.width;
				((Component)component3).transform.localPosition = zero;
				num6 += (float)width;
				zero.x -= (float)width + 5f;
			}
			if (num + num6 + 20f > Widget.localCorners[2].x)
			{
				num2 -= (float)_actionButtons.BaseObject.GetComponent<UIWidget>().height + 10f;
			}
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(Widget.localCorners[2].x - 20f, num2, 0f);
			for (int k = 0; k < count2; k++)
			{
				Transform transform = _actionButtons[k].transform;
				transform.localPosition += val2;
			}
		}
		int height2 = (int)(0f - num2 + 20f);
		Widget.height = height2;
		UIUtility.UpdateAnchors(((Component)this).transform);
		if (height != Widget.height && HeightChanged != null)
		{
			HeightChanged();
		}
	}
}
