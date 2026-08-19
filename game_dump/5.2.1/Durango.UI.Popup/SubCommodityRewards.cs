using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class SubCommodityRewards : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _rewardObjects;

	[SerializeField]
	private UISprite _gridSprite;

	private ListObjectPool<UISprite> _gridSprites;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_gridSprites = new ListObjectPool<UISprite>();
			_gridSprites.BaseObject = _gridSprite;
		}
	}

	public void Set(List<ContentDescription> previews)
	{
		Init();
		_rewardObjects.Set(KUtility.GetSize(previews));
		for (int i = 0; i < _rewardObjects.Count; i++)
		{
			SetItem(_rewardObjects[i], previews[i]);
		}
		UIWidget component = GetComponent<UIWidget>();
		UIWidget component2 = _rewardObjects.BaseObject.GetComponent<UIWidget>();
		int num = component.width / component2.width;
		Vector2 vector = UIUtility.WidgetsGridReposition(_rewardObjects, null, Vector2.down, Vector3.zero, component2.width * num, component2.localSize, 0f, 0f, 0f, new Vector2(0f, 0.5f));
		component.height = Mathf.Max(component2.height, (int)vector.y) + 34;
		UIUtility.MakeGridBackground(Vector3.zero, new Vector2(0f, 0.5f), vector.x, vector.y, component2.localSize, new UIUtility.Separators
		{
			List = _gridSprites,
			Size = 3,
			Bottom = true,
			Left = true,
			Right = true,
			Top = true
		});
	}

	private void SetItem(GameObject obj, ContentDescription item)
	{
		Transform obj2 = obj.transform;
		UISprite component = obj2.Find("IconSprite").GetComponent<UISprite>();
		ItemIconTex component2 = obj2.Find("IconTexture").GetComponent<ItemIconTex>();
		UILabel component3 = obj2.Find("Text").GetComponent<UILabel>();
		if (item.IconColor.Count > 1)
		{
			component2.SetIcon(item.Icon, item.IconColor);
			component.gameObject.SetActive(value: false);
			component2.gameObject.SetActive(value: true);
		}
		else
		{
			component.spriteName = item.Icon;
			component.color = ((item.IconColor.Count <= 0) ? Color.white : item.IconColor[0]);
			component.gameObject.SetActive(value: true);
			component2.gameObject.SetActive(value: false);
		}
		component3.text = item.IconDescription;
	}
}
