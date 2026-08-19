using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Shared.Etc;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopCommodityListWithModular : ShopCommodityListBase, IScreenResizeReceiver
{
	[SerializeField]
	private KScrollView _categoryList;

	[SerializeField]
	private SelectableButton _selectButton;

	[SerializeField]
	private UIWidget _groupedListContainer;

	[SerializeField]
	private ListObjectPool _groupedList;

	[SerializeField]
	private ShopCommodityWidget _infoWidget;

	[SerializeField]
	private UIModelViewer _modelViewer;

	private List<ShopCategory> _categories;

	private int? _selectedIndex;

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		_categoryList.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
	}

	protected override void OnInit()
	{
		base.OnInit();
		_categoryList.Nodes.Init(delegate(GameObject obj)
		{
			Selectable component2 = obj.GetComponent<Selectable>();
			component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickCategory));
		});
		SelectableButton selectButton = _selectButton;
		selectButton.Clicked = (Action)Delegate.Combine(selectButton.Clicked, (Action)delegate
		{
			int? selectedIndex = _selectedIndex;
			if (selectedIndex.HasValue && base.CurrentList != null)
			{
				Durango.Logic.Shop.Commodity commodity = base.CurrentList.Get(_selectedIndex.Value);
				if (commodity != null && Selected != null)
				{
					Selected(commodity.Id);
				}
			}
		});
		_groupedList.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickGroupItem));
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

	private void OnClickGroupItem()
	{
		int num = _groupedList.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			Set(num);
		}
	}

	public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)
	{
		base.SetList(list, reset);
		if (reset)
		{
			_selectedIndex = null;
		}
		int? selectedIndex = _selectedIndex;
		if (!selectedIndex.HasValue)
		{
			_selectedIndex = 0;
		}
		_groupedList.BeginLoad();
		for (int i = 0; i < list.Count; i++)
		{
			Durango.Logic.Shop.Commodity commodity = list[i];
			string text;
			if (KUtility.GetSize(commodity.Contents.Modulars) > 0)
			{
				ModularArtifactContent modularArtifactContent = commodity.Contents.Modulars.First();
				text = $"{modularArtifactContent.size_x} × {modularArtifactContent.size_y}";
			}
			else
			{
				text = commodity.Title;
			}
			GameObject next = _groupedList.GetNext();
			UILabel uILabel = next.FindComponent<UILabel>("Text");
			uILabel.text = text;
		}
		_groupedList.EndLoad();
		Set(_selectedIndex.Value);
		Vector2 vector = UIUtility.WidgetsGridReposition(_groupedList, null, Vector2.down, Vector3.zero, _groupedListContainer.width, _groupedList.BaseObject.GetComponent<UIWidget>().localSize, 10f, 10f, 1f);
		_groupedListContainer.height = (int)vector.y;
	}

	private void Set(int index)
	{
		_selectedIndex = index;
		for (int i = 0; i < _groupedList.Count; i++)
		{
			_groupedList[i].GetComponent<Selectable>().Selected = index == i;
		}
		Durango.Logic.Shop.Commodity commodity = base.CurrentList.Get(index);
		if (commodity == null)
		{
			_infoWidget.gameObject.SetActive(value: false);
			return;
		}
		_infoWidget.gameObject.SetActive(value: true);
		_infoWidget.Set(commodity);
		string text = string.Format("{0}  [preset=round_box?{1}]", T._("구매"), commodity.GetCurrencyText(hasDiscountRatio: false));
		_selectButton.Text = text;
		UILabel uILabel = _selectButton.gameObject.FindComponent<UILabel>("DiscountRate");
		float discountRate = commodity.GetDiscountRate();
		if (uILabel.gameObject.SetActiveAnd(discountRate > 0f))
		{
			uILabel.text = discountRate.ToString("P0");
		}
		if (KUtility.GetSize(commodity.Contents.Modulars) > 0)
		{
			ModularArtifactContent modularArtifactContent = commodity.Contents.Modulars.First();
			_modelViewer.SetArtifactModel(new UIModelViewer.ArtifactArguments
			{
				Display = modularArtifactContent.GetPreview(),
				IsModular = true,
				Rotation = Rotation.None,
				Size = new Point2(modularArtifactContent.size_x, modularArtifactContent.size_y)
			}, new UIModelViewer.Arguments
			{
				CameraAngle = 35f,
				Rotation = -45f
			});
		}
		else
		{
			_modelViewer.gameObject.SetActive(value: false);
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
}
