using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.Logic.Social;
using Durango.Player.Animation;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopCommodityListWithRandomBox : ShopCommodityListBase, IScreenResizeReceiver
{
	[SerializeField]
	private KScrollView _categoryList;

	[SerializeField]
	private NodesScrollView _itemContentsScroll;

	[SerializeField]
	private NodesScrollView _motionContentsScroll;

	[SerializeField]
	private UIWidget _buttonContainer;

	[SerializeField]
	private ListObjectPool _buttonList;

	[SerializeField]
	private ShopCommodityWidget _infoWidget;

	[SerializeField]
	private UIModelViewer _modelViewer;

	[SerializeField]
	private GameObject _emotionalMotionsContainer;

	[SerializeField]
	private UIWidget _emotionalMotionsListWidget;

	[SerializeField]
	private ListObjectPool _emotionalMotionsWidget;

	private Durango.Logic.Shop.Commodity _commodity;

	private List<ShopCategory> _categories;

	private int? _selectedContent;

	private WeightedItemContent _loadingItem;

	private string _selectedMotion;

	private readonly List<string> _emotionalMotions = new List<string>();

	private PlayerAnimationClipInfo _defaultPlayerClip;

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		_categoryList.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
	}

	protected override void OnInit()
	{
		base.OnInit();
		_categoryList.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component5 = obj.GetComponent<Selectable>();
			component5.Clicked = (Action)Delegate.Combine(component5.Clicked, new Action(OnClickCategory));
		});
		_itemContentsScroll.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component4 = obj.GetComponent<Selectable>();
			component4.Clicked = (Action)Delegate.Combine(component4.Clicked, new Action(ItemContentClicked));
		});
		_motionContentsScroll.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component3 = obj.GetComponent<Selectable>();
			component3.Clicked = (Action)Delegate.Combine(component3.Clicked, new Action(MotionContentClicked));
		});
		_emotionalMotionsWidget.Init(delegate(GameObject obj)
		{
			Selectable component2 = obj.GetComponent<Selectable>();
			component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnMotionClick));
		});
		_buttonList.Init(delegate(GameObject obj)
		{
			SelectableButton component = obj.GetComponent<SelectableButton>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickCommodity));
		});
	}

	private void OnClickCategory()
	{
		int num = _categoryList.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1 && CategorySelected != null)
		{
			CategorySelected(_categories[num]);
		}
	}

	private void OnClickCommodity()
	{
		int num = _buttonList.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			Durango.Logic.Shop.Commodity commodity = base.CurrentList[base.CurrentList.Count - 1 - num];
			if (Selected != null)
			{
				Selected(commodity.Id);
			}
		}
	}

	public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)
	{
		base.SetList(list, reset);
		Set(list.FirstOrDefault(), reset);
		_buttonList.BeginLoad();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Durango.Logic.Shop.Commodity commodity = list[num];
			string text = string.Format("{0}  [preset=round_box?{1}]", T._("{0} 회", Mathf.Max(1, commodity.Data.Count)), commodity.GetCurrencyText(hasDiscountRatio: false));
			SelectableButton component = _buttonList.GetNext().GetComponent<SelectableButton>();
			component.Text = text;
			UILabel uILabel = component.gameObject.FindComponent<UILabel>("DiscountRate");
			float discountRate = commodity.GetDiscountRate();
			if (uILabel.gameObject.SetActiveAnd(discountRate > 0f))
			{
				uILabel.text = discountRate.ToString("P0");
			}
		}
		_buttonList.EndLoad();
		UIUtility.WidgetsReposition(_buttonList, _buttonContainer, Vector3.left, 10f);
	}

	private void Set(Durango.Logic.Shop.Commodity item, bool reset)
	{
		_commodity = item;
		if (item == null)
		{
			_infoWidget.gameObject.SetActive(value: false);
			return;
		}
		if (reset)
		{
			_selectedContent = null;
			_selectedMotion = null;
		}
		_infoWidget.gameObject.SetActive(value: true);
		_infoWidget.Set(item);
		if (KUtility.GetSize(item.Contents.WeightedItems) > 0)
		{
			_itemContentsScroll.Nodes.BeginLoad();
			WeightedItemContent[] weightedItems = item.Contents.WeightedItems;
			foreach (WeightedItemContent weightedItemContent in weightedItems)
			{
				if (!weightedItemContent.hide_in_shop)
				{
					GameObject next = _itemContentsScroll.Nodes.GetNext();
					next.transform.Find("Icon").GetComponent<ItemIconTex>().SetIcon(weightedItemContent.prototype_id, weightedItemContent.level);
				}
			}
			_itemContentsScroll.Nodes.EndLoad();
			_itemContentsScroll.Reposition(reset, !reset);
			_itemContentsScroll.gameObject.SetActive(value: true);
			_motionContentsScroll.gameObject.SetActive(value: false);
			int? selectedContent = _selectedContent;
			if (!selectedContent.HasValue)
			{
				SelectItemContent(0, showTooltip: false);
			}
		}
		else if (KUtility.GetSize(item.Contents.WeightedMotions) > 0)
		{
			_motionContentsScroll.Nodes.BeginLoad();
			Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
			string[] weightedMotions = item.Contents.WeightedMotions;
			foreach (string text in weightedMotions)
			{
				Durango.Logic.Social.Motion motion = emotional.GetMotion(text);
				GameObject next2 = _motionContentsScroll.Nodes.GetNext();
				string text2 = ((motion != null) ? ((!motion.IsRare) ? motion.Name : $"<em>{motion.Name}</em>") : text);
				next2.transform.Find("Text").GetComponent<UILabel>().text = text2;
			}
			_motionContentsScroll.Nodes.EndLoad();
			_motionContentsScroll.Reposition(reset, !reset);
			_itemContentsScroll.gameObject.SetActive(value: false);
			_motionContentsScroll.gameObject.SetActive(value: true);
			int? selectedContent2 = _selectedContent;
			if (!selectedContent2.HasValue)
			{
				SelectMotionContent(0);
			}
		}
		else
		{
			_itemContentsScroll.gameObject.SetActive(value: false);
			_motionContentsScroll.gameObject.SetActive(value: false);
		}
	}

	public override void SelectAndMoveTo(string id)
	{
	}

	public override void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)
	{
		_categories = categories;
		_categoryList.Nodes.BeginLoad();
		if (categories != null)
		{
			foreach (ShopCategory category in categories)
			{
				ShopCommodityGroupedTab component = _categoryList.Nodes.GetNext().GetComponent<ShopCommodityGroupedTab>();
				component.Set(category);
				component.Selected = category == selected;
			}
		}
		_categoryList.Nodes.EndLoad();
		_categoryList.ResetPosition();
	}

	public override void RefreshCategoryNotification()
	{
		if (_categories != null)
		{
			for (int i = 0; i < _categories.Count; i++)
			{
				ShopCategory cat = _categories[i];
				base.Parent.GetCategoryNotifiaction(cat, out var on, out var type);
				ShopCommodityGroupedTab component = _categoryList.Nodes[i].GetComponent<ShopCommodityGroupedTab>();
				component.NotificationOn(on, type);
			}
		}
	}

	private void ItemContentClicked()
	{
		int num = _itemContentsScroll.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1 && (!_selectedContent.HasValue || _selectedContent.Value != num))
		{
			SelectItemContent(num, showTooltip: true);
		}
	}

	private void MotionContentClicked()
	{
		int num = _motionContentsScroll.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1 && (!_selectedContent.HasValue || _selectedContent.Value != num))
		{
			SelectMotionContent(num);
		}
	}

	private void SelectItemContent(int index, bool showTooltip)
	{
		_selectedContent = index;
		WeightedItemContent content = _commodity.Contents.WeightedItems.Get(index);
		if (content == null)
		{
			_modelViewer.gameObject.SetActive(value: false);
		}
		else
		{
			if (_loadingItem != content)
			{
				_loadingItem = content;
				PrototypePreset.Request(content.prototype_id, content.level, delegate(PrototypePreset preset)
				{
					if (_loadingItem == content)
					{
						_loadingItem = null;
						ShowItemPreview(preset?.ToItem());
					}
				});
			}
			if (showTooltip)
			{
				Prototype itemPrototype = PrototypeYaml.GetItemPrototype(content.prototype_id, content.level);
				if (itemPrototype != null)
				{
					WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
					widgetTooltipControl.Set(null, $"{itemPrototype.Name} {LocalizeUtil.FormatLevel(content.level)}");
					widgetTooltipControl.Show(5f);
				}
			}
		}
		for (int i = 0; i < _itemContentsScroll.Nodes.Count; i++)
		{
			_itemContentsScroll.Nodes[i].GetComponent<Selectable>().Selected = i == index;
		}
	}

	private void ShowItemPreview(ItemData item)
	{
		_defaultPlayerClip = null;
		if (item != null && item.SetPreview(_modelViewer, delegate
		{
			PlayerBehavior component = _modelViewer.ModelObject.GetComponent<PlayerBehavior>();
			if (!(component == null))
			{
				_defaultPlayerClip = component.CurrentPlayerClipInfo;
			}
		}))
		{
			SetEmotionalMotions(item.EmotionalMotions);
		}
		else
		{
			_modelViewer.gameObject.SetActive(value: false);
		}
	}

	private void SelectMotionContent(int index)
	{
		_selectedContent = index;
		_loadingItem = null;
		SetEmotionalMotions(null);
		string text = _commodity.Contents.WeightedMotions.Get(index);
		Durango.Logic.Social.Motion motion2 = ((text != null) ? GameSystem<SocialSystem>.Instance().Emotional.GetMotion(text) : null);
		if (motion2 == null)
		{
			_modelViewer.gameObject.SetActive(value: false);
		}
		else
		{
			PlayerDisplay display = PlayerBehavior.LocalPlayer.Display;
			display.Equip = null;
			string motion = motion2.MotionNames.Random();
			_modelViewer.SetPlayerModel(PlayerBehavior.LocalPlayer.IsMale, display, new UIModelViewer.Arguments
			{
				CameraAngle = 35f,
				Rotation = 140f,
				Loaded = delegate(GameObject obj)
				{
					PlayerBehavior component = obj.GetComponent<PlayerBehavior>();
					if (!(component == null) && !(component.Anim == null))
					{
						component.PlayMotionForcely(motion, 1f, immediately: true);
					}
				}
			});
		}
		for (int i = 0; i < _motionContentsScroll.Nodes.Count; i++)
		{
			_motionContentsScroll.Nodes[i].GetComponent<Selectable>().Selected = i == index;
		}
	}

	private void SetEmotionalMotions(string[] motions)
	{
		_emotionalMotions.Clear();
		_emotionalMotionsWidget.BeginLoad();
		if (motions != null)
		{
			Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
			foreach (string text in motions)
			{
				Durango.Logic.Social.Motion motion = emotional.GetMotion(text);
				if (motion != null)
				{
					_emotionalMotions.Add(text);
					GameObject next = _emotionalMotionsWidget.GetNext();
					UILabel component = next.transform.Find("Text").GetComponent<UILabel>();
					component.text = motion.Name;
					next.GetComponent<RectLayoutComponent>().UpdateLayout();
				}
			}
		}
		_emotionalMotionsWidget.EndLoad();
		if (_emotionalMotionsWidget.Count > 0)
		{
			_emotionalMotionsContainer.gameObject.SetActive(value: true);
			UIUtility.WidgetsReposition(_emotionalMotionsWidget, _emotionalMotionsListWidget, Vector3.right, 5f);
		}
		else
		{
			_emotionalMotionsContainer.gameObject.SetActive(value: false);
		}
	}

	private void OnMotionClick()
	{
		int num = _emotionalMotionsWidget.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			string text = _emotionalMotions[num];
			if (_selectedMotion == text)
			{
				text = null;
			}
			PlayMotion(text);
		}
	}

	private void PlayMotion(string m)
	{
		_selectedMotion = m;
		Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
		Durango.Logic.Social.Motion motion = emotional.GetMotion(m);
		if (motion == null)
		{
			_selectedMotion = null;
			if (_defaultPlayerClip != null)
			{
				PlayerBehavior component = _modelViewer.ModelObject.GetComponent<PlayerBehavior>();
				if (component != null)
				{
					component.PlayMotionForcely(_defaultPlayerClip.Clip);
				}
			}
		}
		else
		{
			PlayerBehavior component2 = _modelViewer.ModelObject.GetComponent<PlayerBehavior>();
			if (component2 != null)
			{
				component2.PlayMotionForcely(motion.MotionNames.Random());
			}
		}
		for (int i = 0; i < _emotionalMotionsWidget.Count; i++)
		{
			_emotionalMotionsWidget[i].GetComponent<Selectable>().Selected = _emotionalMotions[i] == _selectedMotion;
		}
	}
}
