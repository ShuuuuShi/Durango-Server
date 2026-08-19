using System;
using System.Collections.Generic;
using UnityEngine;

public class MailMenuWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollView;

	private List<Enum> _menuKeys = new List<Enum>();

	private bool _isInit;

	public event Action<Enum> MenuSelected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_scrollView.Nodes.Init(InitMenuNode);
		}
	}

	private void InitMenuNode(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickMenuNode;
	}

	private void OnClickMenuNode(GameObject obj)
	{
		int num = _scrollView.Nodes.IndexOf(obj);
		if (num != -1)
		{
			SelectMenu(num);
			if (this.MenuSelected != null)
			{
				this.MenuSelected(_menuKeys[num]);
			}
		}
	}

	public void SetMenus(Type enumType, params Enum[] ignoreEnums)
	{
		Init();
		Array values = Enum.GetValues(enumType);
		_menuKeys.Clear();
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			Enum @enum = (Enum)values.GetValue(i);
			if (ignoreEnums == null || Array.IndexOf(ignoreEnums, @enum) == -1)
			{
				_menuKeys.Add(@enum);
			}
		}
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.Set(_menuKeys.Count);
		int j = 0;
		for (int count = nodes.Count; j < count; j++)
		{
			GameObject val = nodes[j];
			((Component)val.transform.FindChild("Name")).GetComponent<UISpriteLabel>().text = LocalizeSystem.Get($"#mail_menu_{_menuKeys[j]}");
		}
		_scrollView.Reposition(resetPosition: true, tween: false);
	}

	public void SelectMenu(Enum key)
	{
		int num = _menuKeys.IndexOf(key);
		if (num != -1)
		{
			SelectMenu(num);
		}
	}

	private void SelectMenu(int index)
	{
		int i = 0;
		for (int count = _scrollView.Nodes.Count; i < count; i++)
		{
			PressColorChange component = _scrollView.Nodes[i].GetComponent<PressColorChange>();
			component.Select(i == index);
		}
	}
}
