using System;
using EncyclopediaData;
using UnityEngine;

public class EncyclopediaMemoPage : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabs;

	[SerializeField]
	private EncyclopediaMemoList _memoList;

	[SerializeField]
	private EncyclopediaMemoWidget _memoViewer;

	private MemoType[] _tabTypes;

	private bool _isInitTab;

	public event Action<bool> OnShowMemo;

	private void Awake()
	{
		EncyclopediaMemoList memoList = _memoList;
		memoList.MemoSelected = (Action<MemoType, int>)Delegate.Combine(memoList.MemoSelected, new Action<MemoType, int>(ShowMemo));
	}

	public bool Close()
	{
		if (_memoViewer.IsOpen)
		{
			ShowMemoList(_memoViewer.MemoType, _memoViewer.Index);
			return false;
		}
		return true;
	}

	private void InitTab()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInitTab)
		{
			_isInitTab = true;
			ListObjectPool nodes = _tabs.Nodes;
			nodes.Clear();
			Vector3 localPosition = nodes.BaseObject.transform.localPosition;
			Array values = Enum.GetValues(typeof(MemoType));
			_tabTypes = new MemoType[values.Length];
			int i = 0;
			for (int length = values.Length; i < length; i++)
			{
				MemoType memoType = (MemoType)(int)values.GetValue(i);
				SelectableWidget selectableWidget = ((ListObjectPoolBase<GameObject>)nodes).Add<SelectableWidget>();
				((Object)selectableWidget).name = memoType.ToString();
				_tabTypes[i] = memoType;
				UIEventListener.Get(((Component)selectableWidget).gameObject).onClick = OnSelectTab;
				string key = LocalizeUtil.GetKey(memoType);
				string text = LocalizeSystem.Get(key);
				string spriteName = IconMap.Get(key);
				((Component)((Component)selectableWidget).transform.FindChild("Label")).GetComponent<UILabel>().text = text;
				UISprite component = ((Component)((Component)selectableWidget).transform.FindChild("Icon")).GetComponent<UISprite>();
				component.spriteName = spriteName;
				UIUtility.ResizeToSquare(component, Mathf.Max(component.width, component.height));
				((Component)selectableWidget).transform.localPosition = localPosition;
				localPosition.y -= (float)((Component)selectableWidget).GetComponent<UIWidget>().height;
			}
			_tabs.Reposition(resetPosition: true, tween: false);
		}
	}

	private void OnSelectTab(GameObject obj)
	{
		ListObjectPool nodes = _tabs.Nodes;
		int num = nodes.IndexOf(obj);
		if (num != -1)
		{
			ShowMemoList(_tabTypes[num]);
		}
	}

	public void ShowMemo(MemoType type, int index)
	{
		InitTab();
		_memoList.Hide();
		_memoViewer.ShowMemos(type, index);
		if (this.OnShowMemo != null)
		{
			this.OnShowMemo(obj: true);
		}
	}

	public void ShowMemoList(MemoType type, int initIndex = -1)
	{
		InitTab();
		_memoList.ShowMemos(type, initIndex);
		_memoViewer.Hide();
		int num = Array.IndexOf(_tabTypes, type);
		ListObjectPool nodes = _tabs.Nodes;
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			SelectableWidget component = nodes[i].GetComponent<SelectableWidget>();
			component.Select = i == num;
		}
		if (this.OnShowMemo != null)
		{
			this.OnShowMemo(obj: false);
		}
	}
}
