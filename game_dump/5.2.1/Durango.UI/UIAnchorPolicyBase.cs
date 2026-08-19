using UnityEngine;

namespace Durango.UI;

public abstract class UIAnchorPolicyBase
{
	public abstract void CalculateRootAnchors();

	public abstract void SetBackgroundAnchor(UIBase uiBase);

	public virtual void SetAnchor(UIBase uiBase, UIWidget rootAnchor)
	{
		uiBase.Rect.SetAnchor((!(rootAnchor == null)) ? rootAnchor.gameObject : null, 0, 0, 0, 0);
	}

	public virtual Rect GetSafeRect()
	{
		return DeviceInfo.SafeRect;
	}
}
