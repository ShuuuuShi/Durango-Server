using UnityEngine;

namespace Durango.UI.Control;

[ExecuteInEditMode]
public class TweenColorRecover : TweenColor
{
	[SerializeField]
	private Color _recoverColor;

	protected override void OnDisable()
	{
		base.OnDisable();
		base.value = _recoverColor;
	}
}
