using UnityEngine;

namespace Durango.UI.Popup;

public class ConfirmPopup_PC : ConfirmPopup
{
	[SerializeField]
	private float _topBottomMargin;

	[SerializeField]
	private float _midMargin;

	[SerializeField]
	private float _leftRightMargin;

	[SerializeField]
	private float _buttonMargin;

	[SerializeField]
	private Vector2 _positionRatio;

	protected override void UpdateLayout()
	{
		Vector3 zero = Vector3.zero;
		zero.y = 0f - _topBottomMargin;
		TextLabel.SetPosition(zero, 0.5f, 1f);
		zero.y -= TextLabel.height;
		zero.y -= _midMargin;
		UIUtility.WidgetsReposition(Buttons, Vector3.right, zero + Vector3.down * ButtonBase.Widget.height * 0.5f, _buttonMargin, 0.5f);
		float num = ButtonBase.Widget.width * Buttons.Count;
		if (Buttons.Count > 1)
		{
			num += (float)(Buttons.Count - 1) * _buttonMargin;
		}
		float num2 = ((!(num > (float)TextLabel.width)) ? ((float)TextLabel.width) : num) + _leftRightMargin * 2f;
		base.Widget.width = (int)num2;
		float num3 = 0f - zero.y + (float)ButtonBase.Widget.height + _topBottomMargin;
		base.Widget.height = (int)num3;
		UIUtility.UpdateAnchors(base.transform);
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Base);
		Vector3 position = rootAnchor.GetPosition(_positionRatio.x, _positionRatio.y);
		base.Widget.SetPosition(position, 0.5f, 1f);
	}

	private void LateUpdate()
	{
		UpdateLayout();
	}
}
