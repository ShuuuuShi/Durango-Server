using UnityEngine;

namespace Durango.UI;

public class ArtifactInteriorSetItemTag : MonoBehaviour
{
	private enum ColorType
	{
		Unchecked,
		Checked,
		Complexity
	}

	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private UILabel _textTagName;

	[SerializeField]
	[EnumList(typeof(ColorType), false, 0, -1)]
	private Color[] _colorText;

	[SerializeField]
	private SpriteData _checkIcon;

	[SerializeField]
	private SpriteData _normalIcon;

	public bool IsChecked
	{
		get
		{
			return _checkSprite.spriteName == _checkIcon.sprite;
		}
		private set
		{
			if (value)
			{
				_checkIcon.Set(_checkSprite);
			}
			else
			{
				_normalIcon.Set(_checkSprite);
			}
		}
	}

	public string TagId { get; private set; }

	public void Refresh(string tagId, string tagName)
	{
		TagId = tagId;
		_textTagName.text = tagName;
	}

	public void SetChecked(bool flag)
	{
		IsChecked = flag;
		SetTextColor(flag ? ColorType.Checked : ColorType.Unchecked);
	}

	public void SetComplexity()
	{
		IsChecked = true;
		SetTextColor(ColorType.Complexity);
	}

	private void SetTextColor(ColorType colorType)
	{
		_textTagName.color = _colorText[(int)colorType];
	}
}
