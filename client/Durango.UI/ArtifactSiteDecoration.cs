using UnityEngine;

namespace Durango.UI;

public class ArtifactSiteDecoration : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	private UIWidget _widget;

	private Artifact _artifact;

	private bool _isVisible;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public void Set(Artifact artifact)
	{
		_artifact = artifact;
		_icon.spriteName = artifact.Blueprint.Icon;
	}

	public void Visible(bool visible)
	{
		if (_isVisible != visible)
		{
			_isVisible = visible;
			TweenAlpha tweenAlpha = TweenAlpha.Begin(base.gameObject, 0.2f, (!_isVisible) ? 0f : 1f);
			tweenAlpha.delay = ((!visible) ? 0f : 1f);
		}
	}

	private void OnEnable()
	{
		_isVisible = true;
		Widget.alpha = 0f;
		TweenAlpha tweenAlpha = TweenAlpha.Begin(base.gameObject, 0.2f, 1f);
		tweenAlpha.delay = 1f;
	}
}
