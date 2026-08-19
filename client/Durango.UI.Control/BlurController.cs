using System;
using Durango.System;
using Durango.Utils;

namespace Durango.UI.Control;

public class BlurController : Singleton<BlurController>
{
	public enum Mask
	{
		None,
		BasedOnAnchor,
		Game,
		UI
	}

	private BlurControllerBase _blurController;

	public Mask State => _blurController.GetState();

	public event Action<Mask> BlurStateChanged;

	protected override void OnAwake()
	{
		base.OnAwake();
		if (Platform.Instance.UsePCUI)
		{
			_blurController = new BlurController_PC();
		}
		else
		{
			_blurController = new BlurController_Mobile();
		}
	}

	public static void BlurOn(string key, Mask mask)
	{
		BlurOn(key, mask, UIBase.AnchorType.Default);
	}

	public static void BlurOn(string key, UIBase.AnchorType blurAnchor)
	{
		BlurOn(key, Mask.BasedOnAnchor, blurAnchor);
	}

	private static void BlurOn(string key, Mask mask, UIBase.AnchorType blurAnchor)
	{
		BlurController blurController = Singleton<BlurController>.Instance();
		if (!(blurController == null) && blurController._blurController.BlurOn(key, mask, blurAnchor) && blurController.BlurStateChanged != null)
		{
			blurController.BlurStateChanged(blurController.State);
		}
	}

	public static void BlurOff(string key)
	{
		BlurController blurController = Singleton<BlurController>.Instance();
		if (!(blurController == null) && blurController._blurController.BlurOff(key) && blurController.BlurStateChanged != null)
		{
			blurController.BlurStateChanged(blurController.State);
		}
	}
}
