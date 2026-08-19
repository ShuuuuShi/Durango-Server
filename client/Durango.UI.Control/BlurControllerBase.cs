using Durango.Utils;
using UnityStandardAssets.ImageEffects;

namespace Durango.UI.Control;

public abstract class BlurControllerBase
{
	private Blur _uiBlur;

	protected Blur UIBlur
	{
		get
		{
			if (_uiBlur != null)
			{
				return _uiBlur;
			}
			UICamera uICamera = UICamera.FindCameraForLayer(LayerHelper.UILayer);
			_uiBlur = ((!(uICamera == null)) ? uICamera.GetComponent<Blur>() : null);
			return _uiBlur;
		}
	}

	public abstract BlurController.Mask GetState();

	public abstract bool BlurOn(string key, BlurController.Mask mask, UIBase.AnchorType blurAnchor);

	public abstract bool BlurOff(string key);
}
