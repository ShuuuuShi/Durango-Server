using System;
using UnityEngine;

public class MarketMenuWidget : MonoBehaviour
{
	public enum Tab
	{
		None = -1,
		Buy,
		Sell,
		My
	}

	[SerializeField]
	private ListObjectPool _tabs;

	[SerializeField]
	private int _textPadding;

	private UIWidget _widget;

	public UIWidget Widget => _widget = ((!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>()));

	public event Action<Tab> TabClicked;

	private void Awake()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		_tabs.Init(delegate(GameObject o)
		{
			Selectable component4 = o.GetComponent<Selectable>();
			component4.Clicked = (Action)Delegate.Combine(component4.Clicked, new Action(OnClickTab));
		});
		Array values = Enum.GetValues(typeof(Tab));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			Tab tab = (Tab)(int)values.GetValue(i);
			if (tab != Tab.None)
			{
				SelectableWidget selectableWidget = ((ListObjectPoolBase<GameObject>)_tabs).Add<SelectableWidget>();
				UISpriteLabel component = ((Component)((Component)selectableWidget).transform.FindChild("Name")).GetComponent<UISpriteLabel>();
				UISprite component2 = ((Component)((Component)selectableWidget).transform.FindChild("Icon")).GetComponent<UISprite>();
				string key = LocalizeUtil.GetKey(tab);
				component.text = LocalizeSystem.Get(key);
				component2.spriteName = IconMap.Get(key);
				int length2 = Mathf.Max(component2.width, component2.height);
				UIUtility.ResizeToSquare(component2, length2);
			}
		}
		Vector3 localPosition = _tabs.BaseObject.transform.localPosition;
		int j = 0;
		for (int count = _tabs.Count; j < count; j++)
		{
			UIWidget component3 = _tabs[j].GetComponent<UIWidget>();
			((Component)component3).transform.localPosition = localPosition;
			localPosition.y -= (float)component3.height;
		}
	}

	private void OnClickTab()
	{
		int num = _tabs.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1 && this.TabClicked != null)
		{
			this.TabClicked((Tab)num);
		}
	}

	public void SelectTab(Tab tab)
	{
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			Selectable component = _tabs[i].GetComponent<Selectable>();
			component.Select = tab == (Tab)i;
		}
	}
}
