using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Logic.Market;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MarketCategoriesWidget : MonoBehaviour
{
	[SerializeField]
	private KGridScrollView _categories;

	[LocalizableString]
	[SerializeField]
	private string _viewAllText;

	[SerializeField]
	private SpriteData _viewAllIcon;

	[LocalizableString]
	[SerializeField]
	private string _searchText;

	[SerializeField]
	private SpriteData _searchIcon;

	private UIWidget _widget;

	private Category[] _categoryList;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action<Category.Main> MainCategorySelected;

	public event Action SearchSelected;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_categoryList = GameSystem<MarketSystem>.Instance().CategoryYamlData;
		if (_categoryList == null)
		{
			_isInit = false;
			return;
		}
		if (GameManager.ClusterMode == Mode.Online)
		{
			KeyValuePair<string, string>[] allowedMarketCategoriesInFreeMode = new KeyValuePair<string, string>[7]
			{
				new KeyValuePair<string, string>("accessory", string.Empty),
				new KeyValuePair<string, string>("seed", "seed"),
				new KeyValuePair<string, string>("clothing", string.Empty),
				new KeyValuePair<string, string>("material", "material_building"),
				new KeyValuePair<string, string>("weapon/tool", string.Empty),
				new KeyValuePair<string, string>("building/furniture", string.Empty),
				new KeyValuePair<string, string>("taming", string.Empty)
			};
			_categoryList = _categoryList.Where((Category category) => allowedMarketCategoriesInFreeMode.Any((KeyValuePair<string, string> x) => x.Key == category.MainCategory.Id)).ToArray();
			Category[] categoryList = _categoryList;
			foreach (Category category2 in categoryList)
			{
				string sub = allowedMarketCategoriesInFreeMode.Where((KeyValuePair<string, string> x) => x.Key == category2.MainCategory.Id).FirstOrDefault().Value;
				if (!string.IsNullOrEmpty(sub) && category2.Subs != null)
				{
					category2.Subs = category2.Subs.Where((Category.Sub x) => x.Id == sub).ToArray();
				}
			}
		}
		ListObjectPool nodes = _categories.Nodes;
		nodes.Init(OnInitNodes);
		nodes.Set(_categoryList.Length + 2);
		for (int j = 0; j < _categoryList.Length; j++)
		{
			GameObject obj = nodes[j + 2];
			Category category3 = _categoryList[j];
			obj.transform.Find("name").GetComponent<UILabel>().text = category3.MainCategory.Name;
			obj.transform.Find("icon").GetComponent<UISprite>().spriteName = category3.MainCategory.Icon;
		}
		nodes[0].transform.Find("name").GetComponent<UILabel>().text = T._(_viewAllText);
		nodes[0].transform.Find("icon").GetComponent<UISprite>().spriteName = _viewAllIcon.sprite;
		nodes[1].transform.Find("name").GetComponent<UILabel>().text = T._(_searchText);
		nodes[1].transform.Find("icon").GetComponent<UISprite>().spriteName = _searchIcon.sprite;
		_categories.ResetPosition();
	}

	private void Start()
	{
		Init();
	}

	private void OnInitNodes(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickCategory));
	}

	private void OnClickCategory()
	{
		UISound.PlayClick(UISound.ClickType.ButtonMedium);
		int num = _categories.Nodes.IndexOf(Selectable.Current.gameObject);
		switch (num)
		{
		case 0:
			if (this.MainCategorySelected != null)
			{
				this.MainCategorySelected(null);
			}
			break;
		case 1:
			if (this.SearchSelected != null)
			{
				this.SearchSelected();
			}
			break;
		default:
			if (this.MainCategorySelected != null)
			{
				this.MainCategorySelected(_categoryList[num - 2].MainCategory);
			}
			break;
		}
	}

	public void SelectCategory(Category.Main category)
	{
		int num = -1;
		if (category != null)
		{
			for (int i = 0; i < _categoryList.Length; i++)
			{
				if (_categoryList[i].MainCategory.Id == category.Id)
				{
					num = i + 2;
				}
			}
		}
		ListObjectPool nodes = _categories.Nodes;
		for (int j = 0; j < nodes.Count; j++)
		{
			nodes[j].GetComponent<SelectableWidget>().Selected = num == j;
		}
	}
}
