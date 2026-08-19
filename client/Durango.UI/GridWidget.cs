using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class GridWidget : MonoBehaviour
{
	[SerializeField]
	private int _cellHeight;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UISprite _seperatorSprite;

	private readonly List<SettingItem> _children = new List<SettingItem>();

	public UIWidget Widget => _widget;

	public int GridNumber { get; private set; }

	public void Init(int gridNumber)
	{
		_children.Clear();
		GridNumber = gridNumber;
	}

	public void AddSettingItem(SettingItem item)
	{
		_children.Add(item);
		item.GameObj.transform.SetParent(base.transform);
	}

	public void DetachAllChilds(Transform root)
	{
		foreach (SettingItem child in _children)
		{
			child.GameObj.transform.SetParent(root);
		}
		_children.Clear();
	}

	public void Reposition()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		_seperatorSprite.gameObject.SetActive(!flag);
		UIWidget uIWidget = UIUtility.FindComponentInParent<UIWidget>(base.gameObject);
		int num = ((!flag) ? (uIWidget.width / 2 - 20) : (uIWidget.width - 40));
		for (int i = 0; i < _children.Count; i++)
		{
			SettingItem settingItem = _children[i];
			bool flag2 = ((!flag) ? (i >= _children.Count - 2) : (i >= _children.Count - 1));
			if (settingItem.Setting == null)
			{
				continue;
			}
			settingItem.Widget.width = num;
			if (settingItem.Label != null)
			{
				settingItem.Label.width = num - 100;
				MonoBehaviour monoBehaviour = settingItem.Contents as MonoBehaviour;
				if (monoBehaviour != null)
				{
					GameObject child = monoBehaviour.gameObject;
					if (monoBehaviour.gameObject != null)
					{
						settingItem.Widget.transform.localPosition = Vector3.zero;
						ConfigMainWidget.SetItemChild(settingItem, child, num, !flag2);
					}
				}
			}
			float x = ((flag || i % 2 != 1) ? 20f : ((float)_widget.width / 2f));
			float y = (flag ? (-_cellHeight * i) : (-_cellHeight * (i / 2)));
			settingItem.Widget.transform.localPosition = new Vector3(x, y, 0f);
		}
		int height = ((!flag) ? (((_children.Count - 1) / 2 + 1) * _cellHeight) : (_children.Count * _cellHeight));
		Widget.height = height;
		uIWidget.height = height;
	}
}
