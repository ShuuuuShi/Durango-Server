using Durango.System;

namespace Durango.UI;

public static class UIAnchorPolicy
{
	private static UIAnchorPolicyBase _instance;

	public static float DefaultAspectRatio => 0.5625f;

	public static UIAnchorPolicyBase Instance
	{
		get
		{
			if (_instance == null)
			{
				Initialize();
			}
			return _instance;
		}
	}

	public static void Initialize()
	{
		if (Platform.Instance.UsePCUI)
		{
			_instance = new UIAnchorPolicy_PC();
		}
		else
		{
			_instance = new UIAnchorPolicy_Mobile();
		}
	}
}
