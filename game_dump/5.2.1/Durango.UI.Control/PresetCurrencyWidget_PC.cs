using UnityEngine;

namespace Durango.UI.Control;

public class PresetCurrencyWidget_PC : PresetCurrencyWidget
{
	[SerializeField]
	private UIWidget _bg;

	public override void Init()
	{
		if (!_isInit)
		{
			base.Init();
			UIWidget component = GetComponent<UIWidget>();
			component.rightAnchor.target = null;
			component.ResetAndUpdateAnchors();
		}
	}

	protected override void UpdateLayout()
	{
		base.UpdateLayout();
		_bg.gameObject.SetActive(base.IsButtonActive);
	}

	protected override void OnUpdateWallet()
	{
		base.OnUpdateWallet();
		_bg.UpdateAnchors();
	}
}
