using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class MapActionButtonSelector : TooltipBase
{
	public Action<int> Selected;

	[SerializeField]
	private ListObjectPool _list;

	private readonly List<int> _indexList = new List<int>();

	public int Count => _list.Count;

	protected override void OnAwake()
	{
		base.OnAwake();
		SoundType = UISound.GroupType.NoSound;
		_list.Init(delegate(GameObject obj)
		{
			obj.GetComponent<Selectable>().Clicked = OnClickItem;
		});
	}

	public void BeginLoad()
	{
		_list.BeginLoad();
		_indexList.Clear();
	}

	public void Add(int index, string icon, string text)
	{
		_indexList.Add(index);
		GameObject next = _list.GetNext();
		UISprite component = next.transform.Find("Icon").GetComponent<UISprite>();
		UILabel component2 = next.transform.Find("Text").GetComponent<UILabel>();
		component.spriteName = icon;
		component2.text = text;
		UIUtility.ResizeToSquare(component);
	}

	public void EndLoad()
	{
		_list.EndLoad();
	}

	public UIWidget Get(int index)
	{
		return _list.Get<UIWidget>(index);
	}

	public int GetIndex(int index)
	{
		return _indexList[index];
	}

	protected override void FillData()
	{
		if (_list.Count > 0)
		{
			_list[0].transform.Find("Separator").gameObject.SetActive(value: false);
		}
	}

	protected override void UpdateLayout()
	{
		float num = UIUtility.WidgetsReposition(_list, base.Widget, Vector3.up);
		base.Widget.height = (int)num;
	}

	private void OnClickItem()
	{
		int index = _list.IndexOf(Selectable.Current.gameObject);
		Hide();
		if (Selected != null)
		{
			Selected(_indexList[index]);
		}
	}
}
