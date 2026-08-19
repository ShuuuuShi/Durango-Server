using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class SharedRoutesInfo : MonoBehaviour
{
	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[SerializeField]
	private UILabel _info;

	public void Set([CanBeNull] PlayerInfo info)
	{
		if (info == null || !info.Valid)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		_info.text = T._("<em>{0}</em>\n님의 해도 공유중", info.Name);
		PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
		portraitArgument.Mask = _portraitMaskTexture;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
