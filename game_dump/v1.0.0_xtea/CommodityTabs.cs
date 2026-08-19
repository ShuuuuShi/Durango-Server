using System;
using System.Collections.Generic;
using Messages;
using UnityEngine;

public class CommodityTabs : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabs;

	private IList<Market> _markets;

	private bool _isInit;

	public event Action<Market> TabClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabs.Nodes.Init(delegate(GameObject obj)
			{
				UIEventListener uIEventListener = UIEventListener.Get(obj);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTabClick));
			});
		}
	}

	private void OnTabClick(GameObject obj)
	{
		int num = _tabs.Nodes.IndexOf(obj);
		if (num != -1 && this.TabClicked != null)
		{
			this.TabClicked(_markets[num]);
		}
	}

	public void Set(IList<Market> markets)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_markets = markets;
		ListObjectPool nodes = _tabs.Nodes;
		nodes.Set(KUtility.GetSize(markets));
		for (int i = 0; i < nodes.Count; i++)
		{
			Transform val = nodes[i].transform.FindChild("Text");
			if ((Object)(object)val != (Object)null)
			{
				UILabel component = ((Component)val).GetComponent<UILabel>();
				component.text = markets[i].Name;
			}
			Transform val2 = nodes[i].transform.FindChild("Distance");
			if ((Object)(object)val2 != (Object)null)
			{
				UILabel component2 = ((Component)val2).GetComponent<UILabel>();
				Vector2 val3 = (markets[i].Tile - PlayerBehavior.LocalPlayer.CurrentTile).ToVector2();
				float magnitude = ((Vector2)(ref val3)).magnitude;
				component2.text = $"{magnitude:N0} m";
			}
		}
		_tabs.ResetPosition();
	}

	public void SelectTab(int index)
	{
		ListObjectPool nodes = _tabs.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			Selectable component = nodes[i].GetComponent<Selectable>();
			component.Select = i == index;
		}
	}
}
