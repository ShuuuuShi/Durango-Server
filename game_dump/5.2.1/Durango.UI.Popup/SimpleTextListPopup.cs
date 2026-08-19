using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class SimpleTextListPopup : TooltipBase
{
	private const string BlurKey = "TextListPopup";

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KScrollView _kScrollView;

	private string _title;

	private string[] _textList;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		base.Widget.SetAnchor(base.transform.parent.gameObject, 0.15f, 0.1f, 0.85f, 0.9f);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.transform.localPosition = Vector3.zero;
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_kScrollView.Nodes.BeginLoad();
		if (_textList != null)
		{
			string[] textList = _textList;
			foreach (string text in textList)
			{
				UIWidget component = _kScrollView.Nodes.GetNext().GetComponent<UIWidget>();
				UILabel component2 = component.transform.Find("Label").GetComponent<UILabel>();
				component2.text = text;
				component.height = component2.height + Mathf.Abs(component2.topAnchor.absolute * 2);
			}
		}
		_kScrollView.Nodes.EndLoad();
	}

	protected override void UpdateLayout()
	{
		_kScrollView.ResetPosition();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnShow()
	{
		base.OnShow();
		BlurController.BlurOn("TextListPopup", BlurController.Mask.UI);
	}

	protected override void OnHide()
	{
		base.OnHide();
		BlurController.BlurOff("TextListPopup");
	}

	public void Set(string title, string[] list)
	{
		_title = title;
		_textList = list;
	}
}
