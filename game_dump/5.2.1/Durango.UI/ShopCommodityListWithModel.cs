using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ShopCommodityListWithModel : ShopCommodityList, IScreenResizeReceiver
{
	protected string SelectedId;

	[SerializeField]
	private ShopCommodityWidget _infoWidget;

	[SerializeField]
	private UILabel _previewDescription;

	[SerializeField]
	private UIModelViewer _modelViewer;

	[SerializeField]
	private UILabel _petActiveSkillLabel;

	[SerializeField]
	private KScrollView _petActiveSkills;

	[SerializeField]
	private SelectableButton _buyButton;

	private bool _isWaitPreviewData;

	private Durango.Logic.Shop.Commodity _commodity;

	private readonly List<Messages.PetActiveSkill> _petLearnableSkills = new List<Messages.PetActiveSkill>();

	private List<Messages.PetActiveSkill> _petActiveSkillList;

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		ScrollList.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
	}

	private void Update()
	{
		if (!_isWaitPreviewData)
		{
			return;
		}
		bool flag = true;
		foreach (ContentDescription contentDescription in _commodity.ContentDescriptions)
		{
			if (!contentDescription.IsLoaded)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_isWaitPreviewData = false;
			SetPreview(_commodity);
			_infoWidget.UpdateLayout();
		}
	}

	public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)
	{
		base.SetList(list, reset);
		Select(SelectedId);
		if (string.IsNullOrEmpty(SelectedId) && KUtility.GetSize(base.CurrentList) > 0)
		{
			Select(base.CurrentList[0].Id);
		}
	}

	protected override void OnItemClicked(Durango.Logic.Shop.Commodity item)
	{
		Select(item?.Id);
	}

	public override void SelectAndMoveTo(string id)
	{
		base.SelectAndMoveTo(id);
		Select(id);
	}

	private void Select(string id)
	{
		int num = IndexOf(id);
		SelectedId = ((num != -1) ? base.CurrentList[num].Id : null);
		int i = 0;
		for (int size = KUtility.GetSize(base.CurrentList); i < size; i++)
		{
			Selectable component = ScrollList.Nodes[i].GetComponent<Selectable>();
			if (!(component == null))
			{
				component.Selected = i == num;
			}
		}
		SetCommodity((num != -1) ? base.CurrentList[num] : null);
	}

	private void SetPreview(Durango.Logic.Shop.Commodity commodity)
	{
		_isWaitPreviewData = false;
		ContentDescription conetnt = null;
		if (commodity == null || commodity.TryGetPreviewContent(out conetnt))
		{
			SetItemPreview(conetnt);
			return;
		}
		_isWaitPreviewData = true;
		foreach (ContentDescription contentDescription in commodity.ContentDescriptions)
		{
			contentDescription.Load();
		}
		SetItemPreview(null);
	}

	protected override void OnInit()
	{
		base.OnInit();
		SelectableButton buyButton = _buyButton;
		buyButton.Clicked = (Action)Delegate.Combine(buyButton.Clicked, (Action)delegate
		{
			if (!string.IsNullOrEmpty(SelectedId) && Selected != null)
			{
				Selected(SelectedId);
			}
		});
		_petActiveSkillLabel.text = string.Format("{0} [icon=img_loading_unknown_question1]", T._("특수 행동"));
		UIEventListener uIEventListener = UIEventListener.Get(_petActiveSkillLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, T._("유료 펫만 얻을 수 있는 특수행동이며, 이 이외에도 여러가지 일반 특수 행동을 얻을 수 있습니다."), 500);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			UIWidget childSprite = UIUtility.GetChildSprite(_petActiveSkillLabel, "img_loading_unknown_question1");
			if (childSprite == null)
			{
				widgetTooltipControl.Show(10f);
			}
			else
			{
				widgetTooltipControl.Show(childSprite, Vector2.zero, 10f);
			}
		});
		_petActiveSkills.Nodes.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(obj);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickActiveSkillItem));
		});
	}

	private void SetItemPreview(ContentDescription content)
	{
		bool active = false;
		if (content != null && content.Item != null && content.Item.SetPreview(_modelViewer))
		{
			if (string.IsNullOrEmpty(content.Text))
			{
				_previewDescription.gameObject.SetActive(value: false);
			}
			else
			{
				_previewDescription.gameObject.SetActive(value: true);
				_previewDescription.text = content.Text;
			}
			int petEntityType = content.Item.GetPetEntityType();
			_petActiveSkills.Nodes.BeginLoad();
			if (petEntityType != 0)
			{
				_petLearnableSkills.Clear();
				PetUtil.FindLearnableSkills(_petLearnableSkills, petEntityType);
				_petActiveSkillList = (from x in _petLearnableSkills
					group x by x.SkillId).Select(Enumerable.FirstOrDefault).ToList();
				foreach (Messages.PetActiveSkill petActiveSkill2 in _petActiveSkillList)
				{
					GameObject next = _petActiveSkills.Nodes.GetNext();
					Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(petActiveSkill2.SkillId, petActiveSkill2.Rank);
					next.transform.Find("Icon").GetComponent<UISprite>().spriteName = petActiveSkill?.Icon;
				}
			}
			_petActiveSkills.Nodes.EndLoad();
			_petActiveSkills.ResetPosition();
			active = _petActiveSkills.Nodes.Count > 0;
		}
		else
		{
			_previewDescription.gameObject.SetActive(value: false);
			_modelViewer.gameObject.SetActive(value: false);
		}
		_petActiveSkills.gameObject.SetActive(active);
	}

	private void OnClickActiveSkillItem(GameObject obj)
	{
		int num = _petActiveSkills.Nodes.IndexOf(obj);
		if (num == -1 || num >= KUtility.GetSize(_petActiveSkillList))
		{
			return;
		}
		Messages.PetActiveSkill s = _petActiveSkillList[num];
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (Messages.PetActiveSkill item in from x in _petLearnableSkills
			where x.SkillId == s.SkillId
			orderby x.Rank descending
			select x)
		{
			Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(item.SkillId, item.Rank);
			if (petActiveSkill != null)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += petActiveSkill.Name;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = petActiveSkill.Description;
				}
			}
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(text, text2);
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Show(10f);
		widgetTooltipControl.SetPosition(obj.GetComponent<UIWidget>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector2.up * 20f);
	}

	private void SetCommodity(Durango.Logic.Shop.Commodity item)
	{
		_isWaitPreviewData = false;
		_commodity = item;
		if (item == null)
		{
			_infoWidget.Widget.visible = false;
			return;
		}
		SetPreview(item);
		_infoWidget.Widget.visible = true;
		_infoWidget.Set(_commodity);
		if (item.GetQuestPurchase(CommodityCondition.Type.Level) != null)
		{
			_buyButton.Text = T._("진행중");
			return;
		}
		string arg = ((InventorySystem.Wallet.PurchasableVoucherCount(item) > 0) ? $"{SingletonDict<string, Voucher>.Instance.Get(item.Data.VoucherId).GetIconText()} {item.Data.VoucherAmount}" : (item.IsFree ? T._("무료!") : ((item.Money.Amount <= 0) ? item.Currency : string.Format(T.Culture, "[icon={0}]  {1:N0}", Durango.Logic.Item.Inventory.GetIcon(item.Money.Currency), item.Money.Amount))));
		_buyButton.Text = string.Format("{0}  [preset=round_box?{1}]", T._("구매"), arg);
	}
}
