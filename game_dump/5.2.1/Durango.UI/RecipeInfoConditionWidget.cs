using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class RecipeInfoConditionWidget : MonoBehaviour
{
	[SerializeField]
	private KeyValueLabel _labelBase;

	[SerializeField]
	private UISprite _bgSprite;

	private UIWidget _widget;

	private ListObjectPool<KeyValueLabel> _lists = new ListObjectPool<KeyValueLabel>();

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_widget = GetComponent<UIWidget>();
			_lists.BaseObject = _labelBase;
		}
	}

	public void Set(List<KeyValuePair<string, string>> items)
	{
		Init();
		_lists.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			KeyValuePair<string, string> keyValuePair = items[i];
			KeyValueLabel next = _lists.GetNext();
			next.Set(keyValuePair.Key, keyValuePair.Value);
			next.UpdateLayout(_widget.width);
		}
		_lists.EndLoad();
		int num = Mathf.CeilToInt(_lists.Reposition(Vector3.down, 15));
		_widget.height = num + 40;
	}

	public void SetBackgroundColor(Color color)
	{
		_bgSprite.color = color;
	}
}
