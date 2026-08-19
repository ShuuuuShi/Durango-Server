using UnityEngine;

namespace Durango.UI;

public class UIAnchorPolicy_Mobile : UIAnchorPolicyBase
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
		if (UIManager.IsPortraitScreen)
		{
			CloneGroup cloneGroup = UIManager.FindScript<CloneGroup>();
			PortraitBottomMenuGroup portraitBottomMenuGroup = UIManager.FindScript<PortraitBottomMenuGroup>();
			int num5 = ((!(cloneGroup == null)) ? cloneGroup.BetweenMargin : 0);
			int num6 = ((!(portraitBottomMenuGroup == null)) ? portraitBottomMenuGroup.BottomMenuHeight : 0);
			int num7 = screenWidth - num - num3;
			int num8 = screenHeight - num2 - num4;
			int num9 = num7 * (num8 - (num6 + num5)) / (1280 + num7);
			UIRootAnchor.Reset(UIBase.AnchorType.Default, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.Base, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.CloneFullscreen, num, num2 + num9 + num6 + num5, num3 + (num7 - 1280), num4);
			UIRootAnchor.Reset(UIBase.AnchorType.Clone, num, num2 + num6, num3, num4 + num8 - (num9 + num6));
			UIRootAnchor.Reset(UIBase.AnchorType.Fullscreen, num, num2 + num6, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.FullscreenMobileOnly, num, num2 + num6, num3, num4);
		}
		else
		{
			UIRootAnchor.Reset(UIBase.AnchorType.Default, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.Base, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.Fullscreen, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.CloneFullscreen, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.Clone, num, num2, num3, num4);
			UIRootAnchor.Reset(UIBase.AnchorType.FullscreenMobileOnly, num, num2, num3, num4);
		}
	}

	public override void SetBackgroundAnchor(UIBase uiBase)
	{
		if (!(uiBase.BackgroundWidget == null))
		{
			UIWidget backgroundWidget = uiBase.BackgroundWidget;
			if (!uiBase.IsPortrait || uiBase.Anchor != UIBase.AnchorType.CloneFullscreen)
			{
				backgroundWidget.leftAnchor.SetScreen(0f, 0f);
				backgroundWidget.bottomAnchor.SetScreen(0f, 0f);
				backgroundWidget.rightAnchor.SetScreen(1f, 0f);
				backgroundWidget.topAnchor.SetScreen(1f, 0f);
			}
			else
			{
				Transform transform = uiBase.transform;
				backgroundWidget.leftAnchor.Set(transform, 0f, 0f);
				backgroundWidget.bottomAnchor.Set(transform, 0f, 0f);
				backgroundWidget.rightAnchor.Set(transform, 1f, 0f);
				backgroundWidget.topAnchor.Set(transform, 1f, 0f);
			}
			backgroundWidget.ResetAndUpdateAnchors();
		}
	}

	public override Rect GetSafeRect()
	{
		Rect safeRect = DeviceInfo.SafeRect;
		return (!UIManager.IsPortraitScreen) ? safeRect : new Rect(safeRect.y, safeRect.x, safeRect.height, safeRect.width);
	}
}
