using System;
using System.Collections.Generic;
using MarketData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class TagFilterSearchWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tagList;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private TagFilterSelectorWidget _tagSelector;

	private readonly List<string> _tags = new List<string>();

	private bool _isInit;

	public List<string> Tags => _tags;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tagList.Nodes.Init(InitTagCard);
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_tagSelector.Open(OnSelected, _tags, TagFilterSelectorWidget.ItemType.Tag);
		});
	}

	private void InitTagCard(GameObject obj)
	{
		TagFilterCard component = obj.GetComponent<TagFilterCard>();
		component.Removed = OnRemoveTag;
	}

	private void OnRemoveTag(GameObject obj)
	{
		int num = _tagList.Nodes.IndexOf(obj);
		if (num != -1)
		{
			_tags.RemoveAt(num);
			Refresh(reset: false);
		}
	}

	public void Set(IList<RangeOption> tags)
	{
		Init();
		_tags.Clear();
		if (tags != null)
		{
			for (int i = 0; i < tags.Count; i++)
			{
				_tags.Add(tags[i].Key);
			}
		}
		Refresh(reset: true);
	}

	private void Refresh(bool reset)
	{
		ListObjectPool nodes = _tagList.Nodes;
		nodes.Init(InitPrototypeCard);
		nodes.Set(_tags.Count);
		nodes.Clear();
		for (int i = 0; i < _tags.Count; i++)
		{
			Tag tag = SingletonDict<string, Tag>.Get(_tags[i]);
			if (tag != null)
			{
				TagFilterCard tagFilterCard = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
				tagFilterCard.Set(tag.name);
			}
		}
		_tagList.Reposition(reset, !reset);
	}

	private void InitPrototypeCard(GameObject obj)
	{
		TagFilterCard component = obj.GetComponent<TagFilterCard>();
		component.Removed = (Action<GameObject>)Delegate.Combine(component.Removed, new Action<GameObject>(OnRemoveTag));
	}

	private void OnSelected()
	{
		Refresh(reset: true);
	}
}
