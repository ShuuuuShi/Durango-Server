using System.Collections.Generic;
using Durango.Logic.Item;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuWidget : InteractionMenuWidgetBase
{
	[SerializeField]
	private InteractionMenuQueueList _reservedQueue;

	[SerializeField]
	private UISprite _infoBg;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private UISprite _warningBg;

	private Vector3 _textWidgetPos;

	private Vector3 _infoLabelPos;

	private int _nameFontSize;

	protected override void OnInit()
	{
		base.OnInit();
		_reservedQueue.IconClicked += base.RemoveFirstQueue;
		_textWidgetPos = TextWidget.transform.localPosition;
		_infoLabelPos = InfoLabel.transform.localPosition;
		_nameFontSize = NameLabel.fontSize;
	}

	public override void SetReservedQueueList(List<Pair<int, ItemIcon>> items)
	{
		_reservedQueue.SetList(items, GetSign());
	}

	public override void ClearReservedQueueList()
	{
		_reservedQueue.Clear();
	}

	public override void UpdateUIPosition()
	{
		int sign = GetSign();
		if (sign > 0)
		{
			NameLabel.pivot = UIWidget.Pivot.Left;
			InfoLabel.pivot = UIWidget.Pivot.Left;
			_infoBg.flip = UIBasicSprite.Flip.Nothing;
			_warningLabel.pivot = UIWidget.Pivot.Left;
			_warningBg.flip = UIBasicSprite.Flip.Nothing;
		}
		else
		{
			NameLabel.pivot = UIWidget.Pivot.Right;
			InfoLabel.pivot = UIWidget.Pivot.Right;
			_infoBg.flip = UIBasicSprite.Flip.Horizontally;
			_warningLabel.pivot = UIWidget.Pivot.Right;
			_warningBg.flip = UIBasicSprite.Flip.Horizontally;
		}
		Vector3 textWidgetPos = _textWidgetPos;
		textWidgetPos.x *= sign;
		TextWidget.transform.localPosition = textWidgetPos;
		NameLabel.transform.localPosition = Vector3.zero;
		switch (base.Type)
		{
		case MenuType.Normal:
			NameLabel.fontSize = _nameFontSize;
			break;
		case MenuType.Small:
		{
			float minorScale = InteractionMenuListWidgetBase.MinorScale;
			NameLabel.fontSize = (int)((float)_nameFontSize / minorScale);
			break;
		}
		}
		Vector3 infoLabelPos = _infoLabelPos;
		infoLabelPos.x *= sign;
		if (_warningLabel.gameObject.activeSelf)
		{
			_warningLabel.transform.localPosition = infoLabelPos;
			infoLabelPos.x += sign * (_warningLabel.width + 20);
		}
		if (InfoLabel.gameObject.activeSelf)
		{
			InfoLabel.transform.localPosition = infoLabelPos;
		}
		UIUtility.UpdateAnchors(base.transform);
	}

	public override bool IsWarning()
	{
		return _warningLabel.gameObject.activeSelf;
	}

	protected override void SetWaringText(string text, bool emphasis)
	{
		if (string.IsNullOrEmpty(text))
		{
			_warningLabel.gameObject.SetActive(value: false);
			return;
		}
		_warningLabel.gameObject.SetActive(value: true);
		_warningLabel.text = text;
		_warningLabel.SetEnable<UITweener>(emphasis);
	}
}
