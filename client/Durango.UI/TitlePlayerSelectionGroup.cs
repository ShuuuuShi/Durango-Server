using UnityEngine;

namespace Durango.UI;

public class TitlePlayerSelectionGroup : TitlePlayerSelectionGroupBase
{
	[SerializeField]
	private UIWidget _mainContentWidget;

	[SerializeField]
	private Transform _confirmButtonVrtModeLocation;

	[SerializeField]
	private Transform _confirmButtonHozModeLocation;

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		if (TitleUIRootResizer.IsPortrait)
		{
			_mainContentWidget.SetDimensions(590, 900);
			_confirmButton.Widget.SetAnchor(_confirmButtonVrtModeLocation);
		}
		else
		{
			_mainContentWidget.SetDimensions(900, 500);
			_confirmButton.Widget.SetAnchor(_confirmButtonHozModeLocation);
		}
		UIUtility.UpdateAnchors(_mainContentWidget.transform);
		UIUtility.ResetAndUpdateAnchors(_confirmButton.transform);
	}
}
