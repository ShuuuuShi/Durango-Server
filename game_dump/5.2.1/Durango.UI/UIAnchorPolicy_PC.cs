using UnityEngine;

namespace Durango.UI;

public class UIAnchorPolicy_PC : UIAnchorPolicyBase
{
	public override void CalculateRootAnchors()
	{
		int screenWidth = UIManager.ScreenWidth;
		int screenHeight = UIManager.ScreenHeight;
		Rect safeArea = UIManager.SafeArea;
		int num = (int)(safeArea.xMin * (float)screenWidth);
		int num2 = (int)(safeArea.yMin * (float)screenHeight);
		int num3 = (int)((1f - safeArea.xMax) * (float)screenWidth);
		int num4 = (int)((1f - safeArea.yMax) * (float)screenHeight);
		screenWidth -= num + num3;
		screenHeight -= num2 + num4;
		UIRootAnchor.Reset(UIBase.AnchorType.Default, num, num2, num3, num4);
		UIRootAnchor.Reset(UIBase.AnchorType.Base, num, num2, num3, num4);
		UIRootAnchor.Reset(UIBase.AnchorType.Clone, num, num2, num3, num4);
		int num5 = Mathf.Min(screenWidth, 1500);
		int num6 = Mathf.Min(screenHeight, (int)((float)num5 * UIAnchorPolicy.DefaultAspectRatio));
		num6 -= num6 % 2;
		float f = Mathf.Max(screenWidth - num5, 0f) * 0.5f;
		float f2 = Mathf.Max(screenHeight - num6, 0f) * 0.5f;
		int left = Mathf.CeilToInt(f);
		int right = Mathf.FloorToInt(f);
		int top = Mathf.CeilToInt(f2);
		int bottom = Mathf.FloorToInt(f2);
		UIRootAnchor.Reset(UIBase.AnchorType.Fullscreen, left, bottom, right, top);
		UIRootAnchor.Reset(UIBase.AnchorType.CloneFullscreen, left, bottom, right, top);
		int left2 = screenWidth - 600;
		UIRootAnchor.Reset(UIBase.AnchorType.FullscreenMobileOnly, left2, num2, num3, num4);
	}

	public override void SetBackgroundAnchor(UIBase uiBase)
	{
		if (uiBase.BackgroundWidget == null)
		{
			return;
		}
		UIWidget backgroundWidget = uiBase.BackgroundWidget;
		if (uiBase.Anchor != UIBase.AnchorType.Fullscreen && uiBase.Anchor != UIBase.AnchorType.FullscreenMobileOnly)
		{
			backgroundWidget.leftAnchor.SetScreen(0f, 0f);
			backgroundWidget.bottomAnchor.SetScreen(0f, 0f);
			backgroundWidget.rightAnchor.SetScreen(1f, 0f);
			backgroundWidget.topAnchor.SetScreen(1f, 0f);
		}
		else
		{
			Transform transform = uiBase.transform;
			if (uiBase.Anchor == UIBase.AnchorType.Fullscreen)
			{
				backgroundWidget.leftAnchor.Set(transform, 0f, 0f);
				backgroundWidget.bottomAnchor.Set(transform, 0f, 0f);
				backgroundWidget.rightAnchor.Set(transform, 1f, 0f);
				backgroundWidget.topAnchor.Set(transform, 1f, 0f);
			}
			else
			{
				backgroundWidget.leftAnchor.Set(transform, 0f, 0f);
				backgroundWidget.bottomAnchor.Set(transform, 0f, 0f);
				backgroundWidget.rightAnchor.Set(transform, 1f, 1f);
				backgroundWidget.topAnchor.Set(transform, 1f, 1f);
			}
		}
		backgroundWidget.ResetAndUpdateAnchors();
	}
}
