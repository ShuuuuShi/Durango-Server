using System.Collections.Generic;
using UnityEngine;

public class ReceipeMaterialInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private ListObjectPool _materialItems;

	private UIWidget _widget;

	private string _currentColor;

	private string _slashColor;

	private string _requireColor;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Init()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_currentColor = UIManager.ColorBBCode(UIManager.UIYellow);
			_slashColor = UIManager.ColorBBCode(UIManager.UILightGray);
			_requireColor = UIManager.ColorBBCode(UIManager.UIWhite);
		}
	}

	public void Set(string title, IList<Tuple<string, int, int>> list)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_titleLabel.text = title;
		_materialItems.Set(list.Count);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject obj = _materialItems[i];
			SetItem(obj, list[i]);
		}
		float num = UIUtility.WidgetsReposition(GetChildWidget, _materialItems.Count + 1, Vector3.down, Vector3.zero);
		Widget.height = (int)num;
	}

	private UIWidget GetChildWidget(int index)
	{
		return (index != 0) ? _materialItems[index - 1].GetComponent<UIWidget>() : _titleWidget;
	}

	private void SetItem(GameObject obj, Tuple<string, int, int> data)
	{
		int item = data.Item2;
		int item2 = data.Item3;
		UILabel component = ((Component)obj.transform.FindChild("Material")).GetComponent<UILabel>();
		UILabel component2 = ((Component)obj.transform.FindChild("Count")).GetComponent<UILabel>();
		UISprite component3 = ((Component)obj.transform.FindChild("Icon")).GetComponent<UISprite>();
		component.text = $"[FFFFFF]{data.Item1}[-]";
		component2.text = string.Format("{2}{0}[-] {3}/[-] {4}{1}[-]", item, item2, _currentColor, _slashColor, _requireColor);
		component3.spriteName = ((item >= item2) ? "button_checkbox_selected" : "button_checkbox_normal");
	}
}
