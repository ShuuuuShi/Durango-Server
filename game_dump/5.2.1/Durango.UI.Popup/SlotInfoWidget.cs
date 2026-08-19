using System;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class SlotInfoWidget : UIWidget
{
	private const int Padding = 20;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _levelValueLabel;

	[SerializeField]
	private UILabel _materialLabel;

	[SerializeField]
	private UILabel _materialValueLabel;

	[SerializeField]
	private UILabel _tagLabel;

	[SerializeField]
	private UILabel _tagValueLabel;

	[SerializeField]
	private SelectableButton _searchButton;

	private UIWidget _levelWidget;

	private UIWidget _materialWidget;

	private UIWidget _tagWidget;

	private UIWidget _buttonWidget;

	private bool _isInit;

	public event Action SearchClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_searchButton.Text = string.Format("[icon=market_icon_search:1.4]\n{0}", T._("장터에서 검색"));
		_levelWidget = _levelValueLabel.transform.parent.GetComponent<UIWidget>();
		_materialWidget = _materialValueLabel.transform.parent.GetComponent<UIWidget>();
		_tagWidget = _tagValueLabel.transform.parent.GetComponent<UIWidget>();
		_buttonWidget = _searchButton.transform.parent.GetComponent<UIWidget>();
		SelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, (Action)delegate
		{
			if (this.SearchClicked != null)
			{
				this.SearchClicked();
			}
		});
	}

	public int Set(OrTagFilter tags, OrTagFilter materials, int level, int maxWidth)
	{
		Init();
		_buttonWidget.gameObject.SetActive(GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Market));
		_levelValueLabel.text = T._("{0:lv:} 이상", level);
		_levelWidget.width = Mathf.Max(PredictLabelParentSize(_levelLabel, _levelWidget), PredictLabelParentSize(_levelValueLabel, _levelWidget));
		bool num = tags != null && KUtility.GetSize(tags.Tags) > 0;
		bool flag = materials != null && KUtility.GetSize(materials.Tags) > 0;
		if (num && flag)
		{
			_materialWidget.gameObject.SetActive(value: true);
			_materialLabel.text = T._("재질");
			_materialValueLabel.overflowWidth = maxWidth - _levelWidget.width - _buttonWidget.width - 40;
			_materialValueLabel.text = Util.LocalizedTagRequiredMsg(materials, showLevel: false);
		}
		else
		{
			_materialWidget.gameObject.SetActive(value: false);
		}
		int num2 = (int)Mathf.Abs(_levelValueLabel.GetPosition(0f, 0f).y - _levelWidget.localCorners[1].y) + 20;
		if (num && flag)
		{
			num2 = Mathf.Max(num2, (int)Mathf.Abs(_materialValueLabel.GetPosition(0f, 0f).y - _materialWidget.localCorners[1].y) + 20);
			_materialWidget.height = num2;
		}
		_levelWidget.height = num2;
		_tagValueLabel.overflowWidth = maxWidth - _buttonWidget.width - 40;
		if (!num && flag)
		{
			_tagLabel.text = T._("재질");
			_tagValueLabel.text = Util.LocalizedTagRequiredMsg(materials, showLevel: false);
		}
		else
		{
			_tagLabel.text = T._("속성");
			_tagValueLabel.text = Util.LocalizedTagRequiredMsg(tags, showLevel: false);
		}
		int num3 = (int)Mathf.Abs(_tagValueLabel.GetPosition(0f, 0f).y - _tagWidget.localCorners[1].y) + 20;
		_tagWidget.height = num3;
		base.height = num2 + num3;
		int num4 = _levelWidget.width;
		if (num)
		{
			num4 += Mathf.Max(PredictLabelParentSize(_materialLabel, _materialWidget), PredictLabelParentSize(_materialValueLabel, _materialWidget));
		}
		num4 = Mathf.Max(num4, Mathf.Max(PredictLabelParentSize(_tagLabel, _tagWidget), PredictLabelParentSize(_tagValueLabel, _tagWidget)));
		return num4 + _buttonWidget.width;
	}

	private static int PredictLabelParentSize(UILabel label, UIWidget parent)
	{
		return label.width + (int)(label.GetPosition(0f, 0f).x - parent.localCorners[0].x) * 2;
	}
}
