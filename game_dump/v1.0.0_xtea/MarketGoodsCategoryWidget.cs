using System;
using System.Collections.Generic;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class MarketGoodsCategoryWidget : MonoBehaviour
{
	[SerializeField]
	private KGridScrollView _categories;

	[SerializeField]
	[LocalizableString]
	private string _viewAllText;

	[SerializeField]
	private SpriteData _viewAllIcon;

	[LocalizableString]
	[SerializeField]
	private string _searchText;

	[SerializeField]
	private SpriteData _searchIcon;

	private string[] _categoryList;

	private string[][] _categoryPrototypes;

	private bool _isInit;

	public event Action<string, string[]> CategorySelected;

	public event Action SearchSelected;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		if (SingletonDict<string, List<Prototype>>.Instance == null)
		{
			_isInit = false;
			return;
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<Prototype>> item in SingletonDict<string, List<Prototype>>.Instance)
		{
			List<Prototype> value = item.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			Prototype prototype = value[0];
			if (!string.IsNullOrEmpty(prototype.category))
			{
				List<string> list = dictionary.Get(prototype.category);
				if (list == null)
				{
					list = new List<string>();
					dictionary[prototype.category] = list;
				}
				list.Add(item.Key);
			}
		}
		_categoryList = new string[dictionary.Count];
		_categoryPrototypes = new string[dictionary.Count][];
		int num = 0;
		foreach (KeyValuePair<string, List<string>> item2 in dictionary)
		{
			_categoryList[num] = item2.Key;
			_categoryPrototypes[num] = item2.Value.ToArray();
			num++;
		}
		ListObjectPool nodes = _categories.Nodes;
		nodes.Init(OnInitNodes);
		nodes.Set(_categoryList.Length + 2);
		for (int i = 0; i < _categoryList.Length; i++)
		{
			GameObject val = nodes[i + 2];
			string text = $"#prototype_category_{_categoryList[i]}";
			string text2 = LocalizeSystem.Get(text);
			string spriteName = IconMap.Get(text, "icon_question");
			((Component)val.transform.FindChild("name")).GetComponent<UILabel>().text = text2;
			((Component)val.transform.FindChild("icon")).GetComponent<UISprite>().spriteName = spriteName;
		}
		((Component)nodes[0].transform.FindChild("name")).GetComponent<UILabel>().text = T._(_viewAllText);
		UISprite component = ((Component)nodes[0].transform.FindChild("icon")).GetComponent<UISprite>();
		component.spriteName = _viewAllIcon.sprite;
		((Component)nodes[1].transform.FindChild("name")).GetComponent<UILabel>().text = T._(_searchText);
		component = ((Component)nodes[1].transform.FindChild("icon")).GetComponent<UISprite>();
		component.spriteName = _searchIcon.sprite;
		_categories.ResetPosition();
	}

	private void OnInitNodes(GameObject obj)
	{
		Selectable component = obj.GetComponent<Selectable>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickCategory));
	}

	private void OnClickCategory()
	{
		int num = _categories.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		switch (num)
		{
		case 0:
			if (this.CategorySelected != null)
			{
				this.CategorySelected(null, null);
			}
			break;
		case 1:
			if (this.SearchSelected != null)
			{
				this.SearchSelected();
			}
			break;
		default:
			if (this.CategorySelected != null)
			{
				this.CategorySelected(_categoryList[num - 2], _categoryPrototypes[num - 2]);
			}
			break;
		}
	}

	public void Show()
	{
		Init();
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
