using JetBrains.Annotations;

namespace Durango.UI.Control;

public class LinkIcon : UIWidget, ITextLinkWithValue, ITextLink
{
	private string _link;

	private float? _aspectRatio;

	[UsedImplicitly]
	private void OnClick()
	{
		if (!string.IsNullOrEmpty(_link))
		{
			UIUtility.OpenUri(string.Empty, _link);
		}
	}

	void ITextLinkWithValue.SetPresetValue(string text)
	{
		_link = text;
	}

	public virtual LinkLayoutOption UpdateLayout(TextBuilder builder, int size)
	{
		float? num = _aspectRatio;
		if (!num.HasValue)
		{
			_aspectRatio = aspectRatio;
		}
		SetDimensions((int)((float)size * _aspectRatio.Value), size);
		return default(LinkLayoutOption);
	}
}
