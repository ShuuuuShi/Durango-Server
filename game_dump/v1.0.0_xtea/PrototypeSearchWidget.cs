using System;
using System.Collections.Generic;
using MarketData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class PrototypeSearchWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _prototypeList;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private TagFilterSelectorWidget _prototypeSelector;

	private readonly List<string> _prototype = new List<string>();

	public List<string> Prototype => _prototype;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_prototypeSelector.Open(OnSelected, _prototype, TagFilterSelectorWidget.ItemType.Prototype);
		});
	}

	public void Set(IList<RangeOption> list)
	{
		_prototype.Clear();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				_prototype.Add(list[i].Key);
			}
		}
		Refresh(reset: true);
	}

	private void Refresh(bool reset)
	{
		ListObjectPool nodes = _prototypeList.Nodes;
		nodes.Init(InitPrototypeCard);
		nodes.Clear();
		for (int i = 0; i < _prototype.Count; i++)
		{
			List<Prototype> list = SingletonDict<string, List<Yaml.Prototype>>.Get(_prototype[i]);
			if (list == null)
			{
				continue;
			}
			using List<Prototype>.Enumerator enumerator = list.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Prototype current = enumerator.Current;
				TagFilterCard tagFilterCard = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
				tagFilterCard.Set(current.name);
			}
		}
		_prototypeList.Reposition(reset, !reset);
	}

	private void InitPrototypeCard(GameObject obj)
	{
		TagFilterCard component = obj.GetComponent<TagFilterCard>();
		component.Removed = (Action<GameObject>)Delegate.Combine(component.Removed, new Action<GameObject>(OnRemovePrototype));
	}

	private void OnRemovePrototype(GameObject obj)
	{
		int num = _prototypeList.Nodes.IndexOf(obj);
		if (num != -1)
		{
			_prototype.RemoveAt(num);
			Refresh(reset: false);
		}
	}

	private void OnSelected()
	{
		Refresh(reset: true);
	}
}
