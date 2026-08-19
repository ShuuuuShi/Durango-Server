using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using JetBrains.Annotations;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ItemInfoContainer : MonoBehaviour
{
	[SerializeField]
	private ItemInfoWidget _itemInfoBase;

	[CanBeNull]
	[SerializeField]
	private GameObject _noSelectWidget;

	[SerializeField]
	private int _bottomMargin;

	[SerializeField]
	private bool _enableCraftLink;

	[SerializeField]
	private Color _infoBgColor;

	[SerializeField]
	private Color _detailBgColor;

	[SerializeField]
	private bool _bgBlur;

	private ItemInfoWidget _infoWidget;

	private bool _isInit;

	private UIWidget _cachedWidget;

	private Point2 _widgetSize;

	private KeyValuePair<string, int>? _asyncLoadingPrototypeData;

	public ItemData Item => (!(_infoWidget == null)) ? _infoWidget.CurrentItem : null;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			if (_infoWidget == null)
			{
				_infoWidget = base.gameObject.AddChild(_itemInfoBase.gameObject).GetComponent<ItemInfoWidget>();
				_infoWidget.Init(_enableCraftLink, _infoBgColor, _detailBgColor, _bgBlur);
				_cachedWidget = GetComponent<UIWidget>();
				UIWidget cachedWidget = _cachedWidget;
				cachedWidget.onChange = (Action)Delegate.Combine(cachedWidget.onChange, new Action(OnWidgetChange));
				OnWidgetChange();
				_infoWidget.gameObject.SetActive(value: false);
				UIUtility.ResetAndUpdateAnchors(_infoWidget.transform);
			}
		}
	}

	private void OnWidgetChange()
	{
		Point2 point = new Point2(_cachedWidget.width, _cachedWidget.height);
		if (!(point == _widgetSize))
		{
			_widgetSize = point;
			float num = (float)point.x / (float)_itemInfoBase.width;
			_infoWidget.height = (int)((float)(point.y - _bottomMargin) / num);
			Vector3[] localCorners = _cachedWidget.localCorners;
			Vector3 vector = localCorners[0];
			vector.y += _bottomMargin;
			Vector3 vector2 = localCorners[2];
			Vector2 pivotOffset = _infoWidget.pivotOffset;
			Transform transform = _infoWidget.transform;
			transform.localPosition = new Vector3(Mathf.Lerp(vector.x, vector2.x, pivotOffset.x), Mathf.Lerp(vector.y, vector2.y, pivotOffset.y));
			transform.localScale = Vector3.one * num;
		}
	}

	public void Show(ItemData item, string warnigText = null)
	{
		Init();
		_asyncLoadingPrototypeData = null;
		if (item == null)
		{
			Hide();
			return;
		}
		_infoWidget.SetItemData(item, warnigText);
		_infoWidget.Open();
		if (_noSelectWidget != null)
		{
			_noSelectWidget.SetActive(value: false);
		}
	}

	public void Show(string prototypeId, int level)
	{
		if (_asyncLoadingPrototypeData.HasValue && _asyncLoadingPrototypeData.Value.Key == prototypeId && _asyncLoadingPrototypeData.Value.Value == level)
		{
			return;
		}
		Hide();
		_asyncLoadingPrototypeData = new KeyValuePair<string, int>(prototypeId, level);
		PrototypePreset.Request(prototypeId, level, delegate(PrototypePreset preset)
		{
			if (preset != null)
			{
				KeyValuePair<string, int>? asyncLoadingPrototypeData = _asyncLoadingPrototypeData;
				if (asyncLoadingPrototypeData.HasValue && !(_asyncLoadingPrototypeData.Value.Key != prototypeId) && _asyncLoadingPrototypeData.Value.Value == level)
				{
					Show(preset.ToItem());
				}
			}
		});
	}

	public void Show(Messages.Pet pet, string warnigText = null)
	{
		Init();
		_asyncLoadingPrototypeData = null;
		_infoWidget.SetPetData(pet, warnigText);
		_infoWidget.Open();
		if (_noSelectWidget != null)
		{
			_noSelectWidget.SetActive(value: false);
		}
	}

	public void Hide()
	{
		if (_infoWidget != null)
		{
			_infoWidget.Close();
		}
		if (_noSelectWidget != null)
		{
			_noSelectWidget.SetActive(value: true);
		}
	}
}
